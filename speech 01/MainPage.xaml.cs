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
using Windows.Media.SpeechSynthesis;

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
        private MediaElement startSound, voice;
        private SpeechSynthesizer synthesizer;

        public MainPage()
        {
            this.InitializeComponent();

        }

        private void InitializeListboxVoiceChooser()
        {
            // Get all of the installed voices.
            var voices = SpeechSynthesizer.AllVoices;

            // Get the currently selected voice.
            VoiceInformation currentVoice = synthesizer.Voice;

            foreach (VoiceInformation voice in voices.OrderBy(p => p.Language))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Name = voice.DisplayName;
                item.Tag = voice;
                item.Content = voice.DisplayName + " (Language: " + voice.Language + ")";
                cbVoices.Items.Add(item);

                // Check to see if we're looking at the current voice and set it as selected in the listbox.
                if (currentVoice.Id == voice.Id)
                {
                    item.IsSelected = true;
                    cbVoices.SelectedItem = item;
                }
            }
        }


        #region NATURAL LANGUAGE PROCESSING
        private int evalString(LUISResponse response, string target, string[] mandEntTypes)
        {
            int totScore = 0;
            Debug.WriteLine("[PARSE] - String: " + response.ToString());
            //  Evaluating intents
            foreach (lIntent value in response.intents)
            {
                Debug.WriteLine("[PARSE] - Intent: " + value.intent + " - Score: " + value.score.ToString());
                if ((value.intent == "None") && (value.score > 0.09))
                    totScore--;
                if ((value.intent == target) && (value.score > 0.95))
                    totScore++;
            }
            //  Evaluating entities, there are mandatory entities to look for?
            if (mandEntTypes != null)
            {
                int elemFound = 0, toFind = mandEntTypes.Length;

                foreach (lEntity value in response.entities)
                {
                    var valFound = 0;
                    Debug.WriteLine("[PARSE] - Entity: " + value.entity + " - Type: " + value.type);
                    foreach (string strVal in mandEntTypes)
                    {
                        if (value.type == strVal)
                            valFound++;
                        //  TODO: add confidence evaluation for any entity
                    }
                    if (valFound > 0)
                        elemFound++;
                }
                if (elemFound != toFind)
                    totScore = 0;
                else
                    totScore++;
            }

            return totScore;
        }

        private async Task luisTask(string queryString)
        {
            int evalScore = 0;
            await speechRecognizer.ContinuousRecognitionSession.StopAsync();
            Debug.WriteLine("[LUIS] - Stop recognition");

            using (var client = new HttpClient())
            {
                string uri =
                  "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/66630de7-fd72-44ac-952b-32e6feff975d?subscription-key=6918315f720441f493181e4ca0730294&verbose=true&q=" + queryString;
                HttpResponseMessage msg = await client.GetAsync(uri);
                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var luisData = JsonConvert.DeserializeObject<LUISResponse>(jsonResponse);


                    string[] mTypes = { "Misura", "Ambiente" };


                    evalScore = evalString(luisData, "LetturaSensore", mTypes);
                }

            }

            //  TODO: implement azure iot hub commands

            if (evalScore > 1)
            {
                Debug.WriteLine("[IOTHUB] - Executing command! :-)");
                speakSafe("Ho eseguito il comando");
            }
            else
            {
                Debug.WriteLine("[IOTHUB] - NOT Executing command, low score! :-(");
                speakSafe("Mi spiace, non ho capito");
            }
        }
        #endregion

        #region UI MANAGEMENT & EVENTS

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!started)
            {
                synthesizer = new SpeechSynthesizer();
                InitializeListboxVoiceChooser();
                // Keep track of the UI thread dispatcher, as speech events will come in on a separate thread.
                startSound = new MediaElement();
                voice = new MediaElement();
                dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                started = true;
                speechContext = ResourceContext.GetForCurrentView();
                speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("triggerStrings");
                foreach (var languages in SpeechRecognizer.SupportedTopicLanguages)
                {
                    Debug.WriteLine($"DisplayName: {languages.DisplayName} LanguageTag: {languages.LanguageTag}");
                }
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


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //  TODO:inserire navigazione da e verso la pagina
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }

        public void playSoundSafe()
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                playSound();
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => playSound());
            }
        }

        public void AppendOutputSafe(string input)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                AppendOutput(input);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => AppendOutput(input));
            }
        }

        private async void AppendOutput(string input)
        {
            tbxScript.Text = input  + "\n" + tbxScript.Text;
        }

        public void speakSafe(string textToSay)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                speak(textToSay);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => speak(textToSay));
            }
        }

        public async void speak(string text)
        {
            SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

            // Set the source and start playing the synthesized audio stream.

            voice.SetSource(synthesisStream, synthesisStream.ContentType);
            voice.Play();
        }


        public async void playSound()
        {
            Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            Windows.Storage.StorageFile file = await folder.GetFileAsync("Windows Notify.wav");
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            startSound.SetSource(stream, file.ContentType);
            startSound.Play();
        }
        #endregion

        #region SPEECH RECOGNIZER MANAGEMENT & EVENTS

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

            switch (initType)
            {
                case 0:
                    // Initialize resource map to retrieve localized speech strings.
                    string langTag = recognizerLanguage.LanguageTag;

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


        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("[STATE CHG] - " + args.State.ToString());
          
        }

        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;

            Debug.WriteLine("[HYP] - " + hypothesis);
            AppendOutputSafe(hypothesis);

        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine("[COMPLETED] - " + args.Status.ToString());
            if (args.Status.ToString() == "TimeoutExceeded")
            {
                reconState = 0;
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage, reconState);

                try
                {
                    await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ERR] - On recognize starting...");

                }
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            Debug.WriteLine("[RES] - \"" + args.Result.Text + "\" - Confidence:" + args.Result.Confidence.ToString());
            AppendOutputSafe(args.Result.Text);
            if (args.Result.Confidence != SpeechRecognitionConfidence.Rejected)
            {
                if (reconState == 0)
                {
                    reconState = 1;
                    await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage, reconState);

                    playSoundSafe();
                    
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ERR] - On recognize starting...");

                    }

                    Debug.WriteLine("[RES] - Changing mode: dictate");
                }
                else if (reconState == 1)
                {
                    await luisTask(args.Result.Text);
                    //  Now all the required tasks have been accomplished, we must restart the speech
                    //  recognition engine waiting for a new command
                    reconState = 0;
                    await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage, reconState);

                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ERR] - On recognize starting...");

                    }
                }
            }
        }


        #endregion

        private void cbVoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ComboBoxItem item = (ComboBoxItem)(cbVoices.SelectedItem);
            VoiceInformation voice = (VoiceInformation)(item.Tag);
            synthesizer.Voice = voice;


        }
    }
}