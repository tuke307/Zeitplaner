using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ZeitPlaner.Data;
using ZeitPlaner.Data.Models;

namespace ZeitPlaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime startTime;
        private DateTime stopTime;
        private int bemerkungenAnzahl = 1;
        private static string bemerkung = "Bemerkung {0}: {1} - {2}, Zeitspanne: {3}";
        private static string keinName = "Es wurde noch kein Name angegeben!";
        private static string keinKunde = "Es wurde kein Kunde ausgewählt!";
        private static string keinDatum = "Bitte alle Felder füllen!";
        private static string timerNichtGestartet = "Der Timer wurde noch nicht gestartet!";
        private bool timerLaeuft;
        private SnackbarMessageQueue myMessageQueue;
        private DispatcherTimer timer = new DispatcherTimer();
        private static string dateTimeFormat = "MM/dd/yyyy HH:mm:ss";
        private static string dateTimeToString = "yyyy-MM-dd HH:mm:ss";
        private List<Kunde> Kunden = new List<Kunde>();
        private List<Bemerkung> Bemerkungen = new List<Bemerkung>();
        private int oldKundenListSelectedIndex;
        private int kundenListSelectedIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(Data.Constants.ZeitplanerPath))
            {
                Directory.CreateDirectory(Data.Constants.ZeitplanerPath);
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
        private void kundenHinzufuegen()
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
            if (kundenList.SelectedItem != null)
            {
                startTime = DateTime.Now;

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
                stopTime = DateTime.Now;

                TimeSpan zeitspanne = stopTime - startTime;

                bemerkungenLB.Items.Add(
                    String.Format(
                        bemerkung,
                        bemerkungenLB.Items.Count + 1,
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

        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void timer_Tick(object sender, EventArgs e)
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
            oldKundenListSelectedIndex = kundenListSelectedIndex;
            kundenListSelectedIndex = kundenList.SelectedIndex;

            if (timer.IsEnabled)
            {
                TimerdialogHost.ShowDialog(TimerdialogHost.DialogContent);
                return;
            }

            SeiteNeuLaden();
        }

        /// <summary>
        /// Läd bemeerkungen neu, Läd tagesübersicht neu, Stoppt timer, etc.
        /// </summary>
        private void SeiteNeuLaden()
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

        /// <summary>
        /// Handles the Click event of the loeschenBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
            using (var context = new ZeitplanerDataContext())
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
        private void bemerkungenLaden(Kunde kunde)
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
        public void anzeigenBtn_Click(object sender, RoutedEventArgs e)
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
        private void tagesUebersicht()
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
        private void zeitSpanneUebersicht()
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
        private void datePickerCB_Click(object sender, RoutedEventArgs e)
        {
            endDatumPicker.IsEnabled = !endDatumPicker.IsEnabled;
        }

        /// <summary>
        /// Handles the KeyDown event of the nameTB control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void nameTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                kundenHinzufuegen();
            }
            else if (e.Key == Key.Escape)
            {
            }
        }

        /// <summary>
        /// Handles the Click event of the timerAbbrechenBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void timerAbbrechenBtn_Click(object sender, RoutedEventArgs e)
        {
            stopTime = Convert.ToDateTime(DateTime.Now.ToString(dateTimeToString));

            TimeSpan zeitspanne = stopTime - startTime;

            timer.Stop();
            timerLbl.Content = "0h 0m 0s";

            timerLaeuft = false;

            using (var context = new ZeitplanerDataContext())
            {
                Bemerkung bemerkung = new Bemerkung()
                {
                    StartZeit = startTime,
                    EndZeit = stopTime,
                    KundeID = Kunden[oldKundenListSelectedIndex].ID,
                };
                context.Bemerkung.Add(bemerkung);
                Kunden[oldKundenListSelectedIndex].Bemerkungen.Add(bemerkung);
                context.SaveChanges();

                Bemerkungen.Add(bemerkung);
            }

            bemerkungenAnzahl++;

            SeiteNeuLaden();
        }

        /// <summary>
        /// Handles the Click event of the timerNichtAbbrechenBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void timerNichtAbbrechenBtn_Click(object sender, RoutedEventArgs e)
        {
            kundenList.SelectedIndex = oldKundenListSelectedIndex;
        }
    }
}