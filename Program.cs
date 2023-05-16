using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

class Program
{
    static string speechKeyPrimary = "033452695a2e4036b2c1a11975eb9985";
    //static string speechKeySecondary = "a4efa48dad704e3fa92daba864daa6c0";
    static string speechRegion = "eastus";
    public static string finalText = "";
    static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
    {   
        if (speechRecognitionResult.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine($"Writing...");
        }
        else if (speechRecognitionResult.Reason == ResultReason.NoMatch)
        {
            Console.WriteLine($"Speech could not be recognized!");
        }
        else if (speechRecognitionResult.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

            if (cancellation.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
            }
        }
        finalText += speechRecognitionResult.Text;
        return finalText;
    }

    async static Task Main(string[] args)
    {
        string finalResult = "";
        var speechConfig = SpeechConfig.FromSubscription(speechKeyPrimary, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        //using var audioConfig = AudioConfig.FromWavFileInput(Path.Combine(Directory.GetCurrentDirectory(), "harvard.wav")); //Using audio file
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        Console.WriteLine("Speak into your microphone.");
        bool stopRecognition = false;
        speechRecognizer.Recognizing += (s, e) =>
        {
            Console.WriteLine("Listening...");
            // Check if the recognized text contains the phrase "thank you", then stop recording
            // if (e.Result.Text.ToLower().Contains("thank you"))
            // {
            //     stopRecognition = true;
            // }
        };
        speechRecognizer.Recognized += (s, e) =>
        {
            finalResult = OutputSpeechRecognitionResult(e.Result);
            Console.WriteLine(finalResult);
        };

        await speechRecognizer.StartContinuousRecognitionAsync();
        // while (!stopRecognition)     //Once it stops recognise by saying thank you, this will trigger
        // {
        //     await Task.Delay(1000); // Adjust the delay duration as needed
        // }

        ConsoleKeyInfo keyInfo;        //stop microphone by pressing enter key
        do
        {
            keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                stopRecognition = true;
            }
        } while (!stopRecognition);

        await speechRecognizer.StopContinuousRecognitionAsync();
        var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        OutputSpeechRecognitionResult(speechRecognitionResult);
    }
}