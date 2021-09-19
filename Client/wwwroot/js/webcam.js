const configuration = {
    iceServers: [{
        urls: 'stun:stun.l.google.com:19302' // Google's public STUN server
    }]
};

// create rtc connection with prior configuration
const pc = new RTCPeerConnection(configuration);;
let dotnetReference;

const localDescCreated = (desc) => {
    console.log("Helper function localDescCreated");
    pc.setLocalDescription(
        desc,
        async () => await dotnetReference.invokeMethodAsync('SendRtcData', "sdp", JSON.stringify(pc.localDescription)),
        (error) => console.error(error)
    );
}

window.EnableWebcam = async (dotnetReferenceIn) => {
    try {
        dotnetReference = dotnetReferenceIn;

        const video = document.querySelector("#localUserVideo");

        if (navigator.mediaDevices.getUserMedia)
        {
            console.log("Webcam enabled");
            const localWebcamStream = await navigator.mediaDevices.getUserMedia({ video: true });
            video.srcObject = localWebcamStream;
        }
    }
    catch (error) {
        console.error(error);
    }
};

window.CreateCall = async (isOfferrer) => {
    try {
        console.log("calling create call");
        const remoteVideo = document.querySelector("#remoteUserVideo");

        // 'onicecandidate' notifies us whenever an ICE agent needs to deliver a
        // message to the other peer through the signaling server
        pc.onicecandidate = async (event) => {
            console.log("sending new ice candidate");
            if (event.candidate) {
                await dotnetReference.invokeMethodAsync('SendRtcData', "candidate", JSON.stringify(event.candidate));
            }
        };

        // If user is offerer let the 'negotiationneeded' event create the offer
        if (isOfferrer) {
            console.log("I am the offerer, sending new sdp offer")
            pc.onnegotiationneeded = async () => {
                const description = await pc.createOffer();
                localDescCreated(description);
            }
        }

        // When a remote stream arrives display it in the #remoteVideo element
        pc.onaddstream = (event) => {
            console.log("Adding received video stream");
            remoteVideo.srcObject = event.stream;
        };

        // send local stream to the other user
        console.log("sending local web stream to other user");
        const localWebcamStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        pc.addStream(localWebcamStream)
    }
    catch(error) {
        console.error(error);
    }
};

// This is called after receiving an offer or answer from another peer
window.OnReceiveSdp = (sdp) => {
    console.log("received sdp");
    try {
        // parse json to dict
        sdp = JSON.parse(sdp);

        pc.setRemoteDescription(
            new RTCSessionDescription(sdp),
            async () => {
                // When receiving an offer lets answer it
                if (pc.remoteDescription.type === 'offer') {
                    const desc = await pc.createAnswer();
                    localDescCreated(desc);
                }
            },
            (error) => console.error(error));
    }
    catch (error) {
        console.error(error);
    }
}

// Add the new ICE candidate to our connections remote description
window.OnReceiveIceCandidate = (candidate) => {
    console.log("received ice candidate");
    try {
        // parse json to dict
        candidate = JSON.parse(candidate);

        pc.addIceCandidate(
            new RTCIceCandidate(candidate),
            () => { },
            (error) => console.error(error));
    }
    catch (error) {
        console.error(error);
    }
}
