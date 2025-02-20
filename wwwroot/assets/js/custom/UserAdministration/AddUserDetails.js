var oDocTextTable = null; var docId = 0; var lstValidForSite = null;
var isEdit = 0;

import Enumerable from '../linq.js';
$(document).ready(function () {
    $('#nav_userAdmin').addClass("show");
    $('#menuUser .menu-link').addClass("active");

    CheckePODIndicator();
 
    $("#btnSaveuser").on("click", function () {
        SaveUser();
    });
    $("#chkepodAccess").on("change", function () {
        CheckePODIndicator();
    });

    $("#chkwmsAccess").on("change", function () {
        var WMSAccess = $("#chkwmsAccess").is(":checked");
        if (WMSAccess) {
            if ($('#ddlusersite option:selected').val() == '') {
                Swal.fire('Warning', 'Please select the site.', '');
            }
        }
    });
    _detailpageLoaded = true;

});
function CheckePODIndicator() {
    var ePODAccess = $("#chkepodAccess").is(":checked");
    if (ePODAccess) {
        document.getElementById("ddldriverid").removeAttribute("disabled");
    }
    else {

        $('#ddldriverid').val('').trigger("change");
        document.getElementById("ddldriverid").setAttribute("disabled", "disabled");

    }
}


function SaveUser() {
    var IsSave = true;

    var Usertypeid = $('#ddlUsertype option:selected').val();
    var DriverID = $('#ddldriverid option:selected').val();
    var UserCode = $('#txtUserCode').val();
    var UserName = $('#txtUsername').val();
    var UserEmail = $('#txtEmail').val();
    var password = $('#txtPassword').val();
    var Confirmpassword = $('#txtConfirmPassword').val();
    var Isactive = $("#chkIsActive").is(":checked");
    var ePODAccess = $("#chkepodAccess").is(":checked");
    var coordinatorid = $('#ddlCoordinator').val();
    var wmsAccess = $("#chkwmsAccess").is(":checked");
    var siteid = $('#ddlusersite option:selected').val();
    if (password != Confirmpassword) {
        Swal.fire('Error','Password Does Not Match', 'error');
    }
    if (coordinatorid != '' && ePODAccess == true) {
        Swal.fire('Error', 'User can select either Coordinator or ePOD', 'error'); IsSave = false;
    }
    else if (coordinatorid == '' && ePODAccess == false) {
        Swal.fire('Error', 'Please select either Coordinator or ePOD', 'error'); IsSave = false;

    }
    if (wmsAccess) {
        if ($('#ddlusersite option:selected').val() == '') {
            Swal.fire('Warning', 'Please select the site.', ''); IsSave = false;
        }
    }
    if (IsSave) {
        user.usertype = Usertypeid;
        user.eX_USERCODE = UserCode;
        user.eX_USERNAME = UserName;
        user.eX_EMAILID = UserEmail;
        user.addressid = DriverID;
        user.eX_PASSWORD = password;
        user.siteid = siteid;
        user.isactive = (Isactive === true) ? 1 : 0;
        user.ePOD_Access = (ePODAccess === true) ? 1 : 0;
        user.dashboarD_ACCESS = (wmsAccess === true) ? 1 : 0;
        if (coordinatorid > 0) {
            user.coordinatoR_ID = coordinatorid;
        }
        else {
            user.coordinatoR_ID = null;
        }
        //srcSales_Part_View.expiry_date_required = Requires_Expiry ? 1 : 0;

        $.ajax({
            type: "POST", async: false,
            url: pathname + "/UserAdministration/SaveUserDetails",
            data: { "User": JSON.stringify(user), "ConfirmPassword": Confirmpassword },
            success: function (response) {
                try {
                    if (response.result == true) {
                        var msg = response.msg;

                        Swal.fire({
                            text: msg,
                            icon: "success",
                            buttonsStyling: !1,
                            confirmButtonText: "Ok",
                            customClass: { confirmButton: "btn btn-primary" },
                            showClass: {
                                popup: 'animate__animated animate__fadeInDown'
                            },
                            hideClass: {
                                popup: 'animate__animated animate__fadeOutUp'
                            }
                        }).then((function (e) {
                            $('#btnAddnew').show();
                            setTimeout(function () {
                                $.ajax({
                                    type: "POST", async: true,
                                    url: pathname + "/UserAdministration/showDetails",
                                    data: { "id": response.userid },
                                    success: function (data) {
                                        if (data.result) {

                                            window.location = pathname + "/UserAdministration/UserDetails?ID=" + data.id;
                                        }
                                        else {
                                            Swal.fire('Error', 'Unable to Open Details', 'error');
                                        }
                                        currId = 0;
                                    },
                                });
                            }, 1000);
                        }));

                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Warning', response.msg, 'warning').then((function (e) {
                                $('#btnAddnew').show();
                                InitializeValidForSite();
                            }));
                        }
                        else {
                            Swal.fire('Error', 'Unable to Save User', 'error').then((function (e) {
                                $('#btnAddnew').show();
                                InitializeValidForSite();
                            }));
                        }

                    }
                }
                catch (err) {
                    Swal.fire('Error', 'Unable to Save User', 'error').then((function (e) {
                        $('#btnAddnew').show();
                        InitializeValidForSite();
                    }));
                }
            },
            error: function (response) {

                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                    Swal.fire('Unable to Save User', "Please enter unique code", 'error');
                }
                else Swal.fire('Unable to Save User', response.responseText, 'error');
            }
        });
    }
}



