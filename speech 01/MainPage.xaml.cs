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
using Windows.ApplicationModel.Resources.Core;
using System.Net.Http;
using Newtonsoft.Json;

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
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        private byte reconState = 0;
        private bool started = false;

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
            if (!started)
            {
                // Keep track of the UI thread dispatcher, as speech events will come in on a separate thread.
                dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                started = true;

                speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("triggerStrings");
                // Prompt the user for permission to access the microphone. This request will only happen
                // once, it will not re-prompt if the user rejects the permission.
                bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
                if (permissionGained)
                {
                    btnStart.IsEnabled = true;

                    PopulateLanguageDropdown();
                    await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage, reconState);
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                        Debug.WriteLine("[NAV] - Task creato");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ERR] - On recognize starting...");

                    }
                }
                else
                {
                    this.tbxScript.Text = "Permission to access capture resources was not given by the user, reset the application setting in Settings->Privacy->Microphone.";
                    btnStart.IsEnabled = false;
                    cbLanguage.IsEnabled = false;
                }
            }
        }

        private async Task InitializeRecognizer(Language recognizerLanguage, byte initType)
        {
            if (speechRecognizer != null)
            {
                Debug.WriteLine("[INIT] - Cleanup recognizer");
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
                speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            this.speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            switch(initType)
            {
                case 0:

                    btnStart.IsEnabled = true;

                    // Initialize resource map to retrieve localized speech strings.
                    string langTag = recognizerLanguage.LanguageTag;
                    speechContext = ResourceContext.GetForCurrentView();
                    speechContext.Languages = new string[] { langTag };

                    speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("triggerStrings");

                    try
                    {
                        this.speechRecognizer = new SpeechRecognizer(recognizerLanguage);

                        // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
                        // of an audio indicator to help the user understand whether they're being heard.
                        speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                        // Build a command-list grammar. Commands should ideally be drawn from a resource file for localization, and 
                        // be grouped into tags for alternate forms of the same command.
                        speechRecognizer.Constraints.Add(
                            new SpeechRecognitionListConstraint(
                                new List<string>()
                                {
                        speechResourceMap.GetValue("startCmd", speechContext).ValueAsString
                                }, "Home"));
                        
                        SpeechRecognitionCompilationResult res = await speechRecognizer.CompileConstraintsAsync();
                        if (res.Status != SpeechRecognitionResultStatus.Success)
                        {
                            // Disable the recognition buttons.
                            Debug.WriteLine("Unable to compile grammar!!!");
                        }
                        else
                        {
                            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                            // some recognized phrases occur, or the garbage rule is hit.
                            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                        }
                    }
                    catch (Exception ex)
                    {
                        /*
                        if ((uint)ex.HResult == HResultRecognizerNotFound)
                        {
                            btnContinuousRecognize.IsEnabled = false;

                            resultTextBlock.Visibility = Visibility.Visible;
                            resultTextBlock.Text = "Speech Language pack for selected language not installed.";
                        }
                        else
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                            await messageDialog.ShowAsync();
                        }
                        */
                        Debug.WriteLine("[INIT] - Errore nella try-catch della list");

                    }

                    break;
                case 1:
                    // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
                    // of an audio indicator to help the user understand whether they're being heard.
                    speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                    // Apply the dictation topic constraint to optimize for dictated freeform speech.
                    var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                    speechRecognizer.Constraints.Add(dictationConstraint);
                    SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
                    if (result.Status != SpeechRecognitionResultStatus.Success)
                    {
                        btnStart.IsEnabled = false;
                        Debug.WriteLine("[INIT] - Grammar compilation FAILED");
                    }
                    else
                        Debug.WriteLine("[INIT] - Grammar compilation OK");

                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
                    // allows us to provide incremental feedback based on what the user's currently saying.
                    speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                    speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                    speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
                    break;
            }
            
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //  TODO:inserire navigazione da e verso la pagina
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await LUISParse("ciao");

        }

        private async Task LUISParse(string queryString)
        {
            using (var client = new HttpClient())
            {
                string uri =
                  "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/66630de7-fd72-44ac-952b-32e6feff975d?subscription-key=6918315f720441f493181e4ca0730294&verbose=true&q=\"qual è la temperatura in cucina";// + queryString;
                HttpResponseMessage msg = await client.GetAsync(uri);
                if (msg.IsSuccessStatusCode)
                {
                    
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject<LUISResponse>(jsonResponse);
                    var entityFound = _Data.entities[0].entity;
                    var topIntent = _Data.intents[0].intent;
                    
                }
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
            if (args.Result.Confidence != SpeechRecognitionConfidence.Rejected)
            {
                if (reconState == 0)
                {
                    reconState = 1;
                    await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage, reconState);
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ERR] - On recognize starting...");

                    }

                    Debug.WriteLine("[RES] - Cambio modalità: dettato");
                }
                else if (reconState == 1)
                {
                    //TODO: inserire chiamata verso lui e ritorno a stato list
                }
            }
        }


        #endregion
    }
}
