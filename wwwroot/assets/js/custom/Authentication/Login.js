"use strict";
var pathname = window.location.pathname;
// Class Definition


function login() { 
    var username = document.getElementById('username').value;
    var password = document.getElementById('password').value;

    // Perform login validation
    if (username === '' && password === '') {
        // Successful login, redirect to chat page
        window.location.href = 'chat.html';
    } else {
        // Invalid credentials, display error message
        document.getElementById('error-message').textContent = 'Invalid username or password';
        return false; // Prevent form submission
    }
}
var KTLogin = function () {
    try {
        var _login;

        var _showForm = function (form) {
            var cls = 'login-' + form + '-on';
            var form = 'kt_login_' + form + '_form';

            _login.removeClass('login-forgot-on');
            _login.removeClass('login-signin-on');
            _login.removeClass('login-signup-on');

            _login.addClass(cls);

            //KTUtil.animateClass(KTUtil.getById(form), 'animate__animated animate__backInUp');
        }

        var _handleSignInForm = function () {
            var validation;

            try {
                // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/

                $('#kt_login_signin_submit').on('click', function (e) {
                    e.preventDefault();
                    AuthenticateUser()
                });
            }
            catch (err) {
                alert(err.message);
            }
         
        }

      
        // Public Functions
        return {
            // public functions
            init: function () {
                _login = $('#kt_login');

                _handleSignInForm();
                
            }
        };
    }
    catch (err) {
        alert(err);

    }
}();

// Class Initialization
jQuery(document).ready(function() {
    KTLogin.init();
    if (pathname.indexOf('/') > -1) { pathname = '/'+pathname.split('/')[1]; }
});

function AuthenticateUser() {
    var userid = $('#username').val();
    var password = $('#password').val()
    if (userid == '' || password == '') { alert('Please enter Username and Password.'); }
    else {
        var url = $('#loader').data('request-url');
        
        $.ajax({
            //url: "../assets/data/ORDER_LINES.json?121", async: true, dataType: 'json',              
            //"url": "/Authenticate/AuthenticateUser",
            "url": url,
            //url: "@Url.Action('AuthenticateUser')",
            async: false,       
            "data": { 'UserName': userid, 'Password': password},
            "datatype": "json",
            success: function (response) {
                if (response != "1") alert(response);
                else window.location.href = '/Dashboard/Home';
            },
            failure: function (response) {
                alert(response.responseText);
                
            },
            error: function (response) {
                alert(response.responseText);
            },
        });
    }
   
}
