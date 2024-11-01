using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace WebGLAudioData
{
    public class SpeechDetector : MonoBehaviour
    {
        public float maxVolume = -144f;

        [DllImport("__Internal")]
        public static extern float JS_SpeechDetector_InitOrResumeContext();

        [DllImport("__Internal")]
        public static extern float JS_SpeechDetector_checkForSpeech();

        [DllImport("__Internal")]
        public static extern void JS_SpeechDetector_StopListening();

        public void SetMaxVolume(float maxInput)
        {
            this.maxVolume = maxInput;
        }

        public static void checkForSpeech()
        {
            JS_SpeechDetector_InitOrResumeContext();
            JS_SpeechDetector_checkForSpeech();
        }

        public static void StopListening()
        {
            JS_SpeechDetector_InitOrResumeContext();
            JS_SpeechDetector_StopListening();
        }

    }
}