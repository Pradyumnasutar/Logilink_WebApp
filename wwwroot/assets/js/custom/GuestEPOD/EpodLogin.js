let video = null;
let shipmentid = 0;
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
let cameraPermission = false;
let locationPermission = false;
var pathname = window.location.pathname;
if (window.location.hostname.indexOf('localhost') > -1) pathname = '';
if (pathname.indexOf('/') > -1) { pathname = '/' + pathname.split('/')[1]; }
const x = document.getElementById("msg");

function checkPermissions() {
    return new Promise((resolve, reject) => {
        //let cameraPermission = false;
        //let locationPermission = false;

        // Attempt to access the camera to trigger the permission prompt
        navigator.mediaDevices.getUserMedia({ video: true })
            .then(stream => {
                cameraPermission = true;
                // Stop the camera stream immediately after access
                //stream.getTracks().forEach(track => track.stop());
                checkLocationPermission();
            })
            .catch(error => {
                console.error('Camera permission denied or error accessing camera:', error);
                checkLocationPermission();
            });

        function checkLocationPermission() {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(position => {
                    locationPermission = true;
                    resolve(cameraPermission && locationPermission);
                }, error => {
                    console.error('Location permission denied or error:', error);
                    resolve(cameraPermission && locationPermission);
                });
            } else {
                resolve(cameraPermission && locationPermission);
            }
        }
    });
}

function startCamera() {
    //checkPermissions();
    if (cameraPermission && locationPermission) {

    
        cancelCamerabtn = document.getElementById("cancelCamButton");
        cancelCamerabtn.style.display = 'block';

        container = document.getElementsByClassName('container')[0];
        video = document.getElementById("video");
        video.style.display = 'block';


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

        startButton = document.getElementById("startButton");
        startButton.style.display = 'none';
        container.style.height = "550px";
    }
    else {
        location.reload();
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
    startCamera(); // Restart camera when hiding popup
}

function populateForm(data) {
    try {
        const parsedData = JSON.parse(data);

        const shipmentNo = parsedData.ShipmentNo;
        const vesselName = parsedData.VesselName;
        const customerName = parsedData.CustomerName;
        $('#emailId').val('');
        $('#imono').val('');
        $('#crewName').val('');
        $('#designation').val('');
        if (!shipmentNo || !vesselName || !customerName) {
            // Show a small-sized SweetAlert2 popup for invalid QR code
            Swal.fire({
                title: 'Invalid QR Code',
                text:'Scanned QR code is invalid! Please try again',
                icon: 'warning',
                //confirmButtonText: 'OK',
                //width: '430px',
                //height:'400px',// Adjust the width as needed
                //customClass: {
                //    popup: 'small-swal-popup',
                //    title: 'small-swal-title',
                //    content: 'small-swal-text'
                //}
            }).then((result) => {
                if (result.isConfirmed) {
                    stopCamera();
                    hidePopup();
                }
            });
        } else {
            shipmentid = parsedData.ShipmentId;
            document.getElementById('shipmentNo').value = shipmentNo;
            document.getElementById('vesselName').value = vesselName;
            document.getElementById('customerName').value = customerName;
        }
    } catch (e) {
        console.error("Error parsing QR code data: " + e);
        // Show a small-sized SweetAlert2 popup for JSON parsing error
        Swal.fire({
            title: 'Invalid QR Code',
            text: 'Scanned QR code is invalid! Please try again',
            icon: 'warning',
            //confirmButtonText: 'OK',
            //width: '430px',
            //height: '400px', // Adjust the width as needed
            //customClass: {
            //    popup: 'small-swal-popup',
            //    title: 'small-swal-title',
            //    content: 'small-swal-text'
            //}
        }).then((result) => {
            if (result.isConfirmed) {
                stopCamera();
                hidePopup();
            }
        });
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
    latitude = Str(position.coords.latitude);
    longitude = Str(position.coords.longitude);

}

function loginCrewDetails() {

    var emailid = $('#emailId').val();
    var imono = $('#imono').val();
    var crewName = $('#crewName').val();
    var designation = $('#designation').val();
    var t = true;
    $.ajax({
        type: 'POST',
        async: false,
        url: pathname + '/GuestEPOD/EPODLoginByCrew', data: { "shipmentid": shipmentid, "emailid": emailid, "imono": imono, "crewName": crewName, "designation": designation, "latitude": latitude, "longitude": longitude },
        success: function (ds) {
            ret = ds;
            if (ret != null && ret.result != null && ret.result != undefined && ret.result != true && ret.msg != undefined) {
                $('#msg').text(ret.msg);
            }
            else if (ret != null && ret.result != null && ret.result != undefined && ret.result == true) {
                window.location.href = pathname +'/GuestEPOD/epodshipmentdetails'
            }
        },
        error: function (e) {
            if (t == false) { ret = -1 } else { if (e.responseJSON != undefined) ret = e.responseJSON.Message; else ret = e.responseText; }
        }
    });

}

function Str(v) { if (v == undefined || v == null || v == 'undefined' || v.toString() == '[object Object]' || v.toString() == 'NaN') return ''; else return v.toString().trim(); };
function Int(v) { if (Str(v) == '' || isNaN(parseInt(v))) return 0; else return parseInt(v); };
function Float(v) { if (Str(v) == '' || isNaN(parseFloat(v))) return 0; else return parseFloat(v); };

window.onload = function () {

    getLocation();
    //startCamera(); // Start camera when page loads
    video = document.getElementById("video");
    video.style.display = 'none';

    cancelCamerabtn = document.getElementById("cancelCamButton");
    cancelCamerabtn.style.display = 'none';

   

    $(document).on('click', '#startButton', function (e) {
        e.stopPropagation();
        console.log("start button clicked!");
        checkPermissions().then((permissionsGranted) => {
            if (permissionsGranted) {
                startCamera();
            } else {
                Swal.fire({
                    title: 'To start scanning',
                    html: "Please allow camera and location access<br><span class='smaller-text'>(Refresh the page)</span>",
                    icon: 'warning',

                    confirmButtonText: 'OK',

                }).then((result) => {
                    if (result.isConfirmed) {
                        startCamera();
                        hidePopup(); // Hide popup only if the camera starts
                    }
                });
            }
        });
    })
    
    document.getElementById('cancelCamButton').addEventListener('click', function () {
        stopCamera(); // Stop camera button is clicked
       
    });
    document.getElementById('imono').addEventListener('input', function () {
        const value = this.value;
        if (value.length > 9) {
            // Truncate the value to 9 digits
            this.value = value.slice(0, 9);
        }
    });
    document.getElementById('cancelButton').addEventListener('click', function () {
        hidePopup(); // Hide popup and stop camera when Cancel button is clicked
        $('#shipmentNo').val('');
        $('#vesselName').val('');
        $('#customerName').val('');
        $('#emailId').val('');
        $('#imono').val('');
        $('#crewName').val('');
        $('#designation').val('');
    });
    document.getElementById('confirmButton').addEventListener('click', function () {
        // Retrieve values from input fields
        const emailid = $('#emailId').val().trim();
        const imono = $('#imono').val().trim();
        const crewName = $('#crewName').val().trim();
        const designation = $('#designation').val().trim();

        let errorMsg = '';

        // Check if all fields are filled
        if (!emailid || !imono || !crewName || !designation) {
            errorMsg = 'Please enter all mandatory fields.';
        } else {
            // Validate each field
            if (!validateEmail(emailid)) {
                errorMsg = 'Please enter a valid email address.';
            } else if (!/^\d{1,9}$/.test(imono)) {
                // Validate that IMO No. is up to 9 digits
                errorMsg = 'Please enter a valid IMO No.';
            } else if (!crewName) {
                errorMsg = 'Please enter Crew Name.';
            } else if (!designation) {
                errorMsg = 'Please enter Designation.';
            }
        }

        if (errorMsg) {
            Swal.fire({
                title: 'Validation Error',
                text: errorMsg,
                icon: 'error',
                //confirmButtonText: 'OK',
                //width: '430px',
                //height: '400px',
                //customClass: {
                //    popup: 'small-swal-popup',
                //    title: 'small-swal-title',
                //    content: 'small-swal-text'
                //}
            });
        } else {
            // All fields are valid, proceed with loginCrewDetails
            loginCrewDetails();
            //hidePopup(); // Hide popup and restart scanning
        }
    });

    // Function to validate email
    function validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }



};