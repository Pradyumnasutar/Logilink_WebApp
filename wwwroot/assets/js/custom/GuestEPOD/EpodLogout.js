"use strict";
var pathname = window.location.pathname;
// Class Definition

function Logout() {
    if (window.location.hostname.indexOf('localhost') > -1) pathname = '';
    if (pathname.indexOf('/') > -1) { pathname = '/' + pathname.split('/')[1]; }
    location.href = pathname +"/GuestEPOD/EpodLogin";
}
function MouseOver() {
    document.getElementById("idLoginLink").style.textDecoration = "Underline";
}
function MouseOut() {
    document.getElementById("idLoginLink").style.textDecoration = "None";
}
