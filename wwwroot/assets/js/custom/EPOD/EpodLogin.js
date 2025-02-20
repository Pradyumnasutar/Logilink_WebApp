let video = null;
let startButton = null;
let cancelCamerabtn = null;
let canvasElement = null;
let canvas = null;
let context = null;
let scanning = false;
let stream = null; // Store the stream to stop it properly
let container = null;
let latitude = null;
let longitude = null;
const x = document.getElementById("msg");
function startCamera() {
    container = document.getElementsByClassName('container')[0];
    video = document.getElementById("video");
    video.style.display = 'block';
    video.classList.add('visible');

    cancelCamerabtn = document.getElementById("cancelCamButton");
    cancelCamerabtn.style.display = 'block';
    cancelCamerabtn.classList.add('visible');

    startButton = document.getElementById("startButton");
    startButton.style.display = 'none';
    startButton.classList.add('hide');
    container.style.height = "550px";

    canvasElement = document.getElementById("canvas");
    canvas = canvasElement.getContext("2d");

    // Check if stream exists and is active
    if (stream && stream.active) {
        video.srcObject = stream;
        video.play().then(function () {
            scanning = true;
            requestAnimationFrame(tick);
        });
    } else {
        navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment" } }).then(function (str) {
            stream = str; // Store the stream
            video.srcObject = stream;
            video.setAttribute("playsinline", true); // required to tell iOS safari we don't want fullscreen
            video.play().then(function () {
                scanning = true;
                requestAnimationFrame(tick);
            });
        }).catch(function (err) {
            console.error("Error accessing the camera: " + err);
        });
    }
    
}

function tick() {
    if (scanning && video.readyState === video.HAVE_ENOUGH_DATA) {
        canvasElement.height = video.videoHeight;
        canvasElement.width = video.videoWidth;
        canvas.drawImage(video, 0, 0, canvasElement.width, canvasElement.height);
        const imageData = canvas.getImageData(0, 0, canvasElement.width, canvasElement.height);
        const code = jsQR(imageData.data, imageData.width, imageData.height, {
            inversionAttempts: "dontInvert",
        });
        if (code) {
            document.getElementById("result").textContent = "";
            populateForm(code.data);
            stopCamera(); // Stop scanning after successful code read
            showPopup();
        }
    }
    if (scanning) {
        requestAnimationFrame(tick);
    }
}

function stopCamera() {
    container = document.getElementsByClassName('container')[0];
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
    }
    scanning = false;
    video = document.getElementById("video");
    video.style.display = 'none';
    cancelCamerabtn = document.getElementById("cancelCamButton");
    cancelCamerabtn.style.display = 'none';
    startButton = document.getElementById("startButton");
    startButton.style.display = 'Block';
    container.style.height = "150px";
}

function showPopup() {
    document.getElementById('popup').style.display = 'block';
    video.style.display = 'none'; // Hide video element
}

function hidePopup() {
    document.getElementById('popup').style.display = 'none';
    video = document.getElementById("video");
    video.style.display = 'none';
    cancelCamerabtn = document.getElementById("cancelCamButton");
    cancelCamerabtn.style.display = 'none';
    //startCamera(); // Restart camera when hiding popup
}

function populateForm(data) {
    try {
        const parsedData = JSON.parse(data);
        document.getElementById('shipmentNo').value = parsedData.ShipmentNo || '';
        document.getElementById('vesselName').value = parsedData.VesselName || '';
        document.getElementById('customerName').value = parsedData.CustomerName || '';
    } catch (e) {
        console.error("Error parsing QR code data: " + e);
    }
}

function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
    } else {
        x.innerHTML = "Geolocation is not supported by this browser.";
    }
}

function showPosition(position) {
    latitude = position.coords.latitude;
    longitude = position.coords.longitude;
}

function CrewLogin() {

}

$(document).ready(function () {
    
    getLocation();
    //startCamera(); // Start camera when page loads
    video = $('#video');
    video.css('display', 'none');

    cancelCamerabtn = $('#cancelCamButton');
    cancelCamerabtn.css('display', 'none');

    $('#startButton').click(function (e) {
        startCamera();
        hidePopup(); // Start scanning when Start Scanning button is clicked
    });

    $('#cancelCamButton').click( function (e) {
        stopCamera(); // Stop camera button is clicked
    });


    $('#cancelButton').click( function (e) {
        hidePopup(); // Hide popup and stop camera when Cancel button is clicked
    });

    $('#confirmButton').click( function (e) {
        hidePopup(); // Hide popup and restart scanning when Confirm button is clicked
    });
   
   
});