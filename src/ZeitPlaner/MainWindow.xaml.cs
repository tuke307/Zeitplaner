using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZeitPlaner.Data;
using ZeitPlaner.Data.Models;

namespace ZeitPlaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime startTime;
        DateTime stopTime;
        int bemerkungenAnzahl = 1;
        string bemerkung = "Bemerkung {0}: {1} - {2}, Zeitspanne: {3}";
        string bemerkungVonBis = "{0} - {1}";
        string keinName = "Es wurde noch kein Name angegeben!";
        string keinKunde = "Es wurde kein Kunde ausgewählt!";
        string keinDatum = "Bitte alle Felder füllen!";
        string timerNichtGestartet = "Der Timer wurde noch nicht gestartet!";
        bool timerLaeuft;
        SnackbarMessageQueue myMessageQueue;
        DispatcherTimer timer = new DispatcherTimer();
        string dateTimeFormat = "MM/dd/yyyy HH:mm:ss";
        string dateTimeToString = "yyyy-MM-dd HH:mm:ss";
        List<Kunde> Kunden = new List<Kunde>();
        List<Bemerkung> Bemerkungen = new List<Bemerkung>();


        public MainWindow()
        {
            InitializeComponent();
            if(!Directory.Exists(Data.Constants.DatabasePath))
            {
                Directory.CreateDirectory(Data.Constants.DatabasePath);
            }

            using (var context = new ZeitplanerDataContext())
            {
                // Creates the database if not exists
                context.Database.EnsureCreated();

                context.SaveChanges();
            }

            myMessageQueue = new SnackbarMessageQueue();
            snackBar.MessageQueue = myMessageQueue;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            using (var context = new ZeitplanerDataContext())
            {
                Kunden = context.Kunde.Include(k => k.Bemerkungen).ToList();
                Bemerkungen = context.Bemerkung.ToList();
            }

            kundenList.ItemsSource = Kunden;

            startDatumPicker.SelectedDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy -MM-dd HH:mm:ss"));

        }

        /// <summary>
        /// Kunde hinzufügen / Snackbox anzeigen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            kundenHinzufuegen();
        }

        /// <summary>
        /// Wird ausgeführt, bei AddBtn_Click und wenn in NameTB ENTER gedrückt wird
        /// </summary>
        void kundenHinzufuegen()
        {
            if (!string.IsNullOrEmpty(nameTB.Text))
            {
                Kunden.Add(new Kunde() { Name = nameTB.Text, Bemerkungen = new List<Bemerkung>() });
                kundenList.Items.Refresh();
                using (var context = new ZeitplanerDataContext())
                {
                    context.Kunde.Add(Kunden.Last());
                    context.SaveChanges();
                }

                nameTB.Text = null;
            }
            else
            {
                myMessageQueue.Enqueue(keinName);
            }
        }

        /// <summary>
        /// Timer starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if(kundenList.SelectedItem != null)
            {
                startTime = Convert.ToDateTime(DateTime.Now.ToString(dateTimeToString));

                timerLaeuft = true;
                timer.Start();
            }
            else
            {
                myMessageQueue.Enqueue(keinKunde);
            }
        }

        /// <summary>
        /// Timer stoppen und Bemerkung erstellen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (timerLaeuft)
            {
                stopTime = Convert.ToDateTime(DateTime.Now.ToString(dateTimeToString));

                TimeSpan zeitspanne = stopTime - startTime;

                bemerkungenLB.Items.Add(
                    String.Format(
                        bemerkung, 
                        bemerkungenLB.Items.Count+1, 
                        startTime.ToString(dateTimeFormat), 
                        stopTime.ToString(dateTimeFormat), 
                        zeitspanne.ToString("h'h 'm'm 's's'")));


                timer.Stop();
                timerLbl.Content = "0h 0m 0s";

                timerLaeuft = false;

                using (var context = new ZeitplanerDataContext())
                {
                    Bemerkung bemerkung = new Bemerkung()
                    {
                        StartZeit = startTime,
                        EndZeit = stopTime,
                        KundeID = Kunden[kundenList.SelectedIndex].ID,
                    };
                    context.Bemerkung.Add(bemerkung);
                    Kunden[kundenList.SelectedIndex].Bemerkungen.Add(bemerkung);
                    context.SaveChanges();

                    Bemerkungen.Add(bemerkung);
                }



                bemerkungenAnzahl++;
            }
            else
            {
                myMessageQueue.Enqueue(timerNichtGestartet);
            }
        }


        void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - startTime;

            timerLbl.Content = ts.ToString("h'h 'm'm 's's'");

            if (ts.Hours % 2 == 0 && ts.Hours != 0)
            {
                new ToastContentBuilder()
                    //.AddArgument("Ja", "Ja")
                    //.AddArgument("Nein", "Nein")
                    .AddText("Der Timer läuft noch")
                    .AddText("Arbeiten Sie noch an Kunde")
                    .AddText("'" + Kunden[kundenList.SelectedIndex].Name + "'?")
                    .Show();
            }
        }

        /// <summary>
        /// Bei Wechsel des Kunden Timer stoppen, Bemerkungen laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kundenList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bemerkungenLB.Items.Clear();
            timer.Stop();
            timerLbl.Content = "0h 0m 0s";
            timerLaeuft = false;

            //übersicht aktualisieren
            insgesammtLbl.Content = "0h 0m 0s";
            //vonBisTB.Items.Clear();
            if (kundenList.SelectedIndex >= 0 && expander.IsExpanded)
            {
                if (startDatumPicker.SelectedDate.HasValue && endDatumPicker.IsEnabled == false)
                {
                    tagesUebersicht();
                }
                else
                {
                    zeitSpanneUebersicht();
                }
            }

            if (kundenList.SelectedIndex >= 0)
            {
                //var kunden = context.Kunde.ToArray();
                Kunde kunde = Kunden[kundenList.SelectedIndex];
                bemerkungenLaden(kunde);
            }

        }

        private void loeschenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (kundenList.SelectedItem != null)
            {
                //kundenList.Items.Remove(kundenList.SelectedItem);
                dialogHost.ShowDialog(dialogHost.DialogContent);
            }
            else
            {
                myMessageQueue.Enqueue(keinKunde);
            }
        }

        /// <summary>
        /// Kunde und dazuehörige Bemerkungen werden aus der Liste und der Datenbank gelöscht
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void endgueltigLoeschenBtn_Click(object sender, RoutedEventArgs e)
        {
            using(var context = new ZeitplanerDataContext())
            {
                context.Kunde.Remove(Kunden[kundenList.SelectedIndex]);
                
                Kunden.RemoveAt(kundenList.SelectedIndex);

                kundenList.Items.Refresh();

                context.SaveChanges();
            }
            
        }

        /// <summary>
        /// Bemerkungen ListBox wird aktualisiert
        /// </summary>
        /// <param name="kunde"></param>
        void bemerkungenLaden(Kunde kunde)
        {
            var bemerkungenListe = kunde.Bemerkungen;
            if (bemerkungenListe != null)
            {
                for (int i = 0; i < bemerkungenListe.Count; i++)
                {
                    TimeSpan zeitspanne = Convert.ToDateTime(bemerkungenListe[i].StartZeit) -
                        Convert.ToDateTime(bemerkungenListe[i].EndZeit);

                    bemerkungenLB.Items.Add(
                        String.Format(
                            bemerkung,
                            i + 1,
                            bemerkungenListe[i].StartZeit,
                            bemerkungenListe[i].EndZeit,
                            zeitspanne.ToString("h'h 'm'm 's's'")));
                }
            }
        }

        /// <summary>
        /// Alle Bemerkungen im angegebenem Zeitraum für den anegeebenen Kunden werden ausgegeben 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void anzeigenBtn_Click(object sender,RoutedEventArgs e)
        {
            //vonBisTB.Items.Clear();


            if (kundenList.SelectedIndex < 0)
            {
                myMessageQueue.Enqueue(keinKunde);

                return;
            }


            if (startDatumPicker.SelectedDate.HasValue && endDatumPicker.IsEnabled == false)
            {
                tagesUebersicht();
            }
            else if (startDatumPicker.SelectedDate.HasValue && endDatumPicker.SelectedDate.HasValue)
            {
                zeitSpanneUebersicht();
            }
            else
            {
                myMessageQueue.Enqueue(keinDatum);
            }
            
        }

        /// <summary>
        /// Übersicht eines Tages zu einem Kunden laden
        /// </summary>
        void tagesUebersicht()
        {
            TimeSpan zeitspanneInsgesammt = new TimeSpan();

            DateTime tag = (DateTime)startDatumPicker.SelectedDate;

            DateTime bis = (DateTime)startDatumPicker.SelectedDate;

            //Enddatum
            bis = bis.AddDays(1);

            var kundenBemerkungen = Kunden[kundenList.SelectedIndex].Bemerkungen;

            foreach (Bemerkung bemerkung in kundenBemerkungen)
            {
                if (bemerkung.StartZeit >= tag && bemerkung.EndZeit <= bis)
                {
                    //vonBisTB.Items.Add(
                    //String.Format(
                    //    bemerkungVonBis,
                    //    bemerkung.StartZeit,
                    //    bemerkung.EndZeit));

                    TimeSpan zeitspanne = Convert.ToDateTime(bemerkung.StartZeit) -
                    Convert.ToDateTime(bemerkung.EndZeit);

                    zeitspanneInsgesammt += zeitspanne;
                }
                insgesammtLbl.Content = zeitspanneInsgesammt.ToString("h'h 'm'm 's's'");
            }
        }

        /// <summary>
        /// Übersicht mehrerer Tage zu einem Kunden laden
        /// </summary>
        void zeitSpanneUebersicht()
        {
            TimeSpan zeitspanneInsgesammt = new TimeSpan();

            DateTime von = (DateTime)startDatumPicker.SelectedDate;
            DateTime bis = (DateTime)endDatumPicker.SelectedDate;

            //Enddatum  
            bis = bis.AddDays(1);

            var kundenBemerkungn = Kunden[kundenList.SelectedIndex].Bemerkungen;

            foreach (Bemerkung bemerkung in kundenBemerkungn)
            {
                if (bemerkung.StartZeit >= von && bemerkung.EndZeit <= bis)
                {
                    //vonBisTB.Items.Add(
                    //String.Format(
                    //    bemerkungVonBis,
                    //    bemerkung.StartZeit,
                    //    bemerkung.EndZeit));

                    TimeSpan zeitspanne = Convert.ToDateTime(bemerkung.StartZeit) -
                    Convert.ToDateTime(bemerkung.EndZeit);

                    zeitspanneInsgesammt += zeitspanne;
                }
            }
            insgesammtLbl.Content = zeitspanneInsgesammt.ToString("h'h 'm'm 's's'");
        }


        /// <summary>
        /// Zweites Datumfeld enablen / disablen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void datePickerCB_Click(object sender, RoutedEventArgs e)
        {
            endDatumPicker.IsEnabled = !endDatumPicker.IsEnabled;
        }

        private void nameTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                kundenHinzufuegen();
            }
            else if(e.Key == Key.Escape)
            {
            }
        }
    }
}
