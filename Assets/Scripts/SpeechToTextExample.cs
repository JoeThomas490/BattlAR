using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

public class SpeechToTextExample : MonoBehaviour
{
    public GridCellManager gridCellManager;
    public AuthorizeSelection authorizeSelection;

    public Text titleText;

    public AudioSource confirmBeepSource;
    public RectTransform listenPanel;

    public bool m_bIsAuthorizing = false;

    public float m_fSilenceThreshold = 0.75f;


    private string _username = "a6e150d4-0394-4ada-b5ff-21e4b4df4909";
    private string _password = "6nscMRqKJA5k";
    private string _url = "https://stream-tls10.watsonplatform.net/speech-to-text/api";

    private int m_RecordingRoutine = 0;


    private AudioClip m_Recording = null;
    private int m_RecordingBufferSize = 2;
    private int m_RecordingHZ = 22050;


    private SpeechToText m_SpeechToText;

    void Start()
    {
        //Create the credentials to pass to the speech to text service
        Credentials credentials = new Credentials(_username, _password, _url);

        //Instantiate a new SpeechToText object
        m_SpeechToText = new SpeechToText(credentials);


        LogSystem.InstallDefaultReactors();
        Log.Debug("ExampleStreaming", "Start();");

        Active = false;
        //StartRecording();
    }

    public bool Active
    {
        get { return m_SpeechToText.IsListening; }
        set
        {
            //If set to true and we're not listening
            if (value && !m_SpeechToText.IsListening)
            {
                m_SpeechToText.DetectSilence = true;
                m_SpeechToText.EnableWordConfidence = false;
                m_SpeechToText.EnableTimestamps = false;
                m_SpeechToText.SilenceThreshold = m_fSilenceThreshold;
                m_SpeechToText.MaxAlternatives = 5;
                //Continous recognition off to only record single words
                m_SpeechToText.EnableContinousRecognition = false;
                m_SpeechToText.EnableInterimResults = false;
                m_SpeechToText.OnError = OnError;
                m_SpeechToText.StartListening(OnRecognize);
            }
            else if (!value && m_SpeechToText.IsListening)
            {
                m_SpeechToText.StopListening();
            }
        }
    }

    public void TriggerListen()
    {
        if (Active)
        {
            Active = false;
            listenPanel.gameObject.SetActive(false);
            Debug.Log("Deactivating listen!");
            StopRecording();
        }
        else
        {
            Active = true;
            listenPanel.gameObject.SetActive(true);
            Debug.Log("Listening for voice!");
            StartRecording();
        }
    }

    private void StartRecording()
    {
        //If there isn't a recording co-routine running
        if (m_RecordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            m_RecordingRoutine = Runnable.Run(RecordingHandler());
            Debug.Log("Start recording..");
        }
    }

    private void StopRecording()
    {
        if (m_RecordingRoutine != 0)
        {
            Microphone.End(null);
            Runnable.Stop(m_RecordingRoutine);
            m_RecordingRoutine = 0;
            Debug.Log("Stop recording..");
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Debug.Log("Error listening to voice : " + error);

        //Log.Debug("ExampleStreaming", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        m_Recording = Microphone.Start(null, true, m_RecordingBufferSize, m_RecordingHZ);
        Debug.Log("Initialising microphone");
        yield return null;      // let m_RecordingRoutine get set..


        //If the recording doesn't initialise properly
        if (m_Recording == null)
        {
            

            //Stop recording
            StopRecording();
            //Break out of function
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = m_Recording.samples / 2;
        float[] samples = null;

        //While our recording routine is still running and the recording isn't null
        while (m_RecordingRoutine != 0 && m_Recording != null)
        {
            //Get the position to write to
            int writePos = Microphone.GetPosition(null);
            //If we are going to overload the samples array or the mic isn't recording anymore
            if (writePos > m_Recording.samples || !Microphone.IsRecording(null))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");

                //Stop recording
                StopRecording();
                yield break;
            }

            //Recording is done in two halves for some reason
            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                m_Recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(samples);
                record.Clip = AudioClip.Create("Recording", midPoint, m_Recording.channels, m_RecordingHZ, false);
                record.Clip.SetData(samples, 0);

                m_SpeechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (m_Recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)m_RecordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }


    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if(AuthorizeSelection.m_bIsAuthorizing)
        {
            return;
        }

        if (result != null && result.results.Length > 0)
        {
            confirmBeepSource.Play();
            Active = false;
            listenPanel.gameObject.SetActive(false);

            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = alt.transcript;
                    Log.Debug("ExampleStreaming", string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));
                    Debug.Log("Heard : " + text);

                    //we want to disect text here
                    if (!m_bIsAuthorizing)
                    {
                        if (TestString(text))
                        {
                            //titleText.text = "Cell name recognized!";
                            break;
                        }
                        else
                        {
                            //titleText.text = "No cell name recognized in speech. Please try again!";
                        }
                    }

                }
            }
        }
    }

    private bool TestString(string testString)
    {
        string[] cutUpText = testString.Split(' ');

        //Debug.Log("Text received : " + cutUpText);

        if (cutUpText[0].Equals("debug") || cutUpText[0].Equals("console"))
        {
            ARShips.Console.instance.show = !ARShips.Console.instance.show;
            return false;
        }

        int num = SearchForNumber(cutUpText);
        char c = SearchForLetter(cutUpText);

        string tag = c + num.ToString();

        Debug.Log("Testing string : " + tag);

        GridCell cell = gridCellManager.GetGridCellByTag(tag);

        if (cell != null)
        {
            if (cell.gameObject != null)
            {
                authorizeSelection.Authorize(tag);
                return true;
            }
        }
        return false;
    }

    int SearchForNumber(string[] strings)
    {
        foreach (string s in strings)
        {
            switch (s)
            {
                case "one":
                case "want":
                    return 1;
                case "two":
                case "to":
                case "too":
                    return 2;
                case "three":
                    return 3;
                case "four":
                case "for":
                    return 4;
                case "five":
                    return 5;
                case "six":
                    return 6;
                case "seven":
                    return 7;
                case "eight":
                case "ate":
                    return 8;
                case "nine":
                    return 9;
                case "ten":
                    return 10;
            }
        }

        return -1;
    }

    char SearchForLetter(string[] strings)
    {
        foreach (string s in strings)
        {
            switch (s.ToLower())
            {
                case "a":
                case "alpha":
                case "alfa":
                    return 'A';
                case "be":
                case "beta":
                    return 'B';
                case "see":
                case "the":
                case "charlie":
                case "charli":
                    return 'C';
                case "delta":
                case "d":
                case "dee":
                    return 'D';
                case "he":
                case "echo":
                    return 'E';
                case "eff":
                case "foxtrot":
                    return 'F';
                case "golf":
                    return 'G';
                case "hotel":
                    return 'H';
                case "india":
                    return 'I';
                case "juliet":
                case "juliett":
                    return 'J';
            }
            if (s.Length == 2)
            {
                if (s[1] == '.')
                    return s[0];
            }
        }

        return ' ';
    }
}
