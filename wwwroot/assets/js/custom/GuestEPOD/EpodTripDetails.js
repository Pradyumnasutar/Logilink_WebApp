$(document).ready(function () {

        getCurrentLocation()
            .then((location) => {
                if (location) {
                    ViewDetails( location.latitude, location.longitude);
                    
                } else {
                    Swal.fire('Permission Error', 'Location access was denied. Please enable location services and try again.', 'error')
                    console.log('Location access was denied. Please enable location services and try again.');
                }
            })
            .catch((error) => {
                console.error('Error occurred:', error);
                Swal.fire('Permission Error', 'An error occurred while retrieving location. Please try again later.', 'error');

            });
    //}
    //else {
    //    Swal.fire('Error!', "Something didn't go as planned.Please refresh the page or come back later.", 'error');
    //}
    
});

function getCurrentLocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const coordinates = {
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude
                    };
                    resolve(coordinates);
                },
                (error) => {
                    if (error.code === error.PERMISSION_DENIED) {
                        resolve(false);
                    } else {
                        reject(error);
                    }
                }
            );
        } else {
            reject(new Error("Geolocation is not supported by this browser."));
        }
    });
}

function ViewDetails( longitude, latitude) {
    // Create a form element
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = redirecturl // Adjust action and controller

    // Function to create a hidden input field
    const createHiddenInput = (name, value) => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        return input;
    };

    // Append hidden inputs to the form
    form.appendChild(createHiddenInput('longitude', longitude));
    form.appendChild(createHiddenInput('latitude', latitude));

    // Add the form to the document and submit it
    document.body.appendChild(form);
    form.submit();
}
