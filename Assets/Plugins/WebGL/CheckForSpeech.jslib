var SpeechDetector = {
    JS_SpeechDetector_InitOrResumeContext: function () {
        if (!WEBAudio || WEBAudio.audioWebEnabled == 0) {
            // No WEBAudio object (Unity version changed?)
            return false;
        }

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            navigator.getUserMedia =
                navigator.getUserMedia || navigator.webkitGetUserMedia ||
                navigator.mozGetUserMedia || navigator.msGetUserMedia;
            if (!navigator.getUserMedia) {
                return false;
            }
        }

        var sdCtx = document.speechDetectorContext;
        if (!sdCtx) {
            document.speechDetectorContext = {};
            sdCtx = document.speechDetectorContext;
        }

        if (!sdCtx.audioContext || sdCtx.audioContext.state == "closed") {
            sdCtx.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }

        if (sdCtx.audioContext.state == "suspended") {
            sdCtx.audioContext.resume();
        }

        if (sdCtx.audioContext.state == "suspended") {
            return false;
        }

        return true;
    },


    JS_SpeechDetector_checkForSpeech: function () {
        let maxVolume = -144.0
        var sdCtx = document.speechDetectorContext;

        var handleStream = function (userMediaStream) {
            var stream = {};
            stream.userMediaStream = userMediaStream;

            stream.microphone = sdCtx.audioContext.createMediaStreamSource(stream.userMediaStream);
            stream.analyser = sdCtx.audioContext.createAnalyser();
            stream.analyser.smoothingTimeConstant = 0.8;


            stream.microphone.connect(stream.analyser);
            stream.analyser.fftSize = 256;

            const dataArray = new Float32Array(stream.analyser.frequencyBinCount);

            setTimeout(() => {
                stream.analyser.getFloatFrequencyData(dataArray)

                for (i = 0; i < dataArray.length; i++) {
                    if (dataArray[i] > maxVolume && dataArray[i] < 0) {
                        maxVolume = dataArray[i];
                        myInstance.SendMessage('SpeechRecognition', 'SetMaxVolume', maxVolume);

                    }
                }
            }, 250)

            sdCtx.stream = stream;
            return maxVolume
        }

        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ audio: true })
                .then(function (umStream) {
                    maxVolume = handleStream(umStream);

                }).catch(function (e) { console.error(e.name + ": " + e.message); });
        } else {
            navigator.getUserMedia({ audio: true },
                function (umStream) { handleStream(umStream); },
                function (e) { console.error(e.name + ": " + e.message); });
        }

        return maxVolume;
    },
    JS_SpeechDetector_StopListening: function () {
        var sdCtx = document.speechDetectorContext;
        if (sdCtx && sdCtx.stream) {
            sdCtx.stream.analyser.disconnect();
            sdCtx.stream.microphone.disconnect();

            delete sdCtx.stream;

        }
    },


}


mergeInto(LibraryManager.library, SpeechDetector);