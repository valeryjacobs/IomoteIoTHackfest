using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.SpeechRecognition;
using Windows.Globalization;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Media.Capture;
using System.Threading.Tasks;

// Il modello di elemento per la pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace speech_01
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //  Private members
        private CoreDispatcher dispatcher;
        // The speech recognizer used throughout this sample.
        private SpeechRecognizer speechRecognizer;

        public MainPage()
        {
            this.InitializeComponent();

        }


        private void PopulateLanguageDropdown()
        {
            Language defaultLanguage = SpeechRecognizer.SystemSpeechLanguage;
            IEnumerable<Language> supportedLanguages = SpeechRecognizer.SupportedTopicLanguages;
            foreach (Language lang in supportedLanguages)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = lang;
                item.Content = lang.DisplayName;

                cbLanguage.Items.Add(item);
                if (lang.LanguageTag == defaultLanguage.LanguageTag)
                {
                    item.IsSelected = true;
                    cbLanguage.SelectedItem = item;
                }
            }
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            // Keep track of the UI thread dispatcher, as speech events will come in on a separate thread.
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // Prompt the user for permission to access the microphone. This request will only happen
            // once, it will not re-prompt if the user rejects the permission.
            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                btnStart.IsEnabled = true;

                PopulateLanguageDropdown();
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            }
            else
            {
                this.tbxScript.Text = "Permission to access capture resources was not given by the user, reset the application setting in Settings->Privacy->Microphone.";
                btnStart.IsEnabled = false;
                cbLanguage.IsEnabled = false;
            }
        }

        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                //  TODO: gestione cambio di stato del recognizer speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
                //  TODO: gestione cambio di stato del recognizer speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                //  TODO: gestione cambio di stato del recognizer speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                //  TODO: gestione cambio di stato del recognizer speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            this.speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
            // of an audio indicator to help the user understand whether they're being heard.
            speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

            // Apply the dictation topic constraint to optimize for dictated freeform speech.
            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
            speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                // TODO: inserire gestione notifiche rootPage.NotifyUser("Grammar Compilation Failed: " + result.Status.ToString(), NotifyType.ErrorMessage);
                btnStart.IsEnabled = false;
                Debug.WriteLine("Grammar compilation FAILED");
            }
            else
                Debug.WriteLine("Grammar compilation OK");

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //  TODO:inserire navigazione da e verso la pagina
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERR] - On recognize starting...");
                
            }

        }

        #region RECOGNIZER EVENTS
        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("[STATE CHG] - " + args.State.ToString());
        }

        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;

            Debug.WriteLine("[HYP] - " + hypothesis);

        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine("[COMPLETED] - " + args.Status.ToString());
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            Debug.WriteLine("[RES] - \"" + args.Result.Text + "\" - Confidence:" + args.Result.Confidence.ToString());
        }
        #endregion
    }
}
