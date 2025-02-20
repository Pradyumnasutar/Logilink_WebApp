
$(document).ready(function () {
    $('#nav_userAdmin').addClass("show");
    $('#menuUser .menu-link').addClass("active");
    $('#btnSaveNew').on('click', function (e) {
        e.preventDefault();
        SaveNewUser();
    });
    $('#createddt').val(Date.now())
    $('#createddt').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y", minDate: "today"
    });

});

function SaveNewUser() {
    var IsSave = true;


    var formData = {
        UserDetails: {
            
            ex_usercode: $('#txtusername').val(),
            ex_username: $('#txtfirstname').val(),
            created_date: convertToDate($('#createddt').val()),
            ex_emailid: $('#txtEmail').val(),
            //companyid: $('#txtcompany').val(),
            //usertype: $('#txtrole').val(),
            password_expiry_date: $('#txtPwdExpiry').val(),
            isactive: 0,
            usertype: $('#txtrole option:selected').val(),
            companyid: $('#txtcompany option:selected').val()
        },
        Password: $('#txtpassword').val(),
        ConfirmPassword: $('#txtconfirmpassword').val(),
        IsNewUser: 1

    };



    console.log(formData);
    
    if (formData.Password != formData.ConfirmPassword) {
        Swal.fire('Error', 'Password Does Not Match', 'error');
        IsSave = false;
    }
    
    if (IsSave) {
        $.ajax({
            url: pathname + '/UserAdministration/SaveUser',
            type: 'POST',

            data: { "userData": JSON.stringify(formData) },
            success: function (data) {
                console.log(data)
                if (data.result) {
                    Swal.fire({
                        title: 'Success',
                        text: 'New User created Successfully!',
                        icon: 'success',
                        showCancelButton: false,
                        confirmButtonText: 'Ok'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            
                            OpenUserDetails(data.data)
                            //window.location.reload();
                        }
                    });


                } else {
                    Swal.fire('Failed', data.message, 'error');
                }
            },
            error: function (data) { // Corrected the error function syntax
                Swal.fire('Error', data.message, 'error');
            }
        });


    }
}

function OpenUserDetails(data) {
    //console.log(data)
    $.ajax({
        url: pathname + '/Outbound/EncryptId',
        type: 'GET',
        data: { id: data.ex_userid },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/UserAdministration/UserDetails?userId=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}