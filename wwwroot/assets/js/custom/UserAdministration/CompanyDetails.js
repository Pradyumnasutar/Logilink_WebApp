
$(document).ready(function () {


    $('#nav_userAdmin').addClass("show");
    $('#menuCompany .menu-link').addClass("active");
    var addressid = window.addressId;
   
    setCompanyLogo(companyInfo);
    loadSMAddressDetails(addressid);
    loadAddressDropdown();

    var imgInput = KTImageInput.getInstance(document.querySelector('#imgLogo'));
    var imgfaviconInput = KTImageInput.getInstance(document.querySelector('#imgFavicon'));
    var imgprintInput = KTImageInput.getInstance(document.querySelector('#imgPrintLogo'));

    imgInput.on("kt.imageinput.change", handleImageChange('#imgLogoInput'));
    imgfaviconInput.on("kt.imageinput.change", handleImageChange('#imgFaviconInput'));
    imgprintInput.on("kt.imageinput.change", handleImageChange('#imgPrintLogoInput'));


    $('#ddlCompanyAddress').on('change', function () {
        var addressid = $(this).val();
        loadSMAddressDetails( addressid);
        /*var selectedAddressId = $(this).val();
        SaveCompany(companyInfo, selectedAddressId);*/
    });

    $('#saveButton').on("click", function () {
        SaveCompany(companyInfo);
    });


   
});
function handleImageChange(inputSelector) {
    return function (e) {
        var file = document.querySelector(inputSelector).files[0];
        if (file) {
            convertImageToBase64(file, function (base64Data) {
            });
        }
    };
}



function loadAddressDropdown() {
    $.ajax({
        url: pathname+'/UserAdministration/GetAllSMAddress',
        type: 'GET',
        data: {},
        success: function (response) {
            var dropdown = $('#ddlCompanyAddress');
            dropdown.empty();
            

            $.each(response.data, function (index, address) {
                dropdown.append($('<option>', {
                    value: address.addressid,
                    text: `${address.addr_code} - ${address.addr_name}`
                }));
            });
            dropdown.val(window.addressId);
        },
        error: function (xhr, status, error) {
            console.error('Error loading address data:', error);
        }
    });
}

function loadSMAddressDetails(addressid) {

    $.ajax({
        url: pathname+'/UserAdministration/GetSMaddressDetails',
        type: 'GET',
        data: { addressid: addressid },
        success: function (response) {
            if (response.data) {
                $('#txtAddressType').val(response.data.addr_type);
                $('#txtAddress').val([response.data.address1, response.data.address2, response.data.address3, response.data.address4].filter(Boolean).join(', '));
                $('#txtPostalCode').val(response.data.addr_zipcode);
                $('#txtCountry').val(response.data.addr_country);
                $('#txtContactPerson').val(response.data.contact_person);
                $('#txtPhone').val(response.data.addr_phone1);
                $('#txtMobile').val(response.data.addr_mobilephone);
                $('#txtEmail').val(response.data.addr_email);
                $('#txtFax').val(response.data.addr_fax);
                $('#txtCompCode').val(companyInfo.Company_Code);
                $('#txtCompDesc').val(companyInfo.Company_Description);
                
            } else {
                console.error('No details found for the selected address.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading address details:', error);
        }
    });
}

function setCompanyLogo(companyInfo) {
    if (companyInfo) {
        // Set main logo
        if (companyInfo.base64Logo) {
            var mainLogoMimeType = getMimeType(companyInfo.base64Logo);
            var mainLogoUrl = "data:" + mainLogoMimeType + ";base64," + companyInfo.base64Logo;
            $('#imgLogo .image-input-wrapper').css('background-image', 'url(' + mainLogoUrl + ')');
            $('#imgLogo').attr('alt', companyInfo.companyDescription || 'Company Logo');
        } else {
            $('#imgLogo .image-input-wrapper').css('background-image', '');
        }

        // Set favicon
        if (companyInfo.base64minLogo) {
            var faviconMimeType = getMimeType(companyInfo.base64minLogo);
            var faviconUrl = "data:" + faviconMimeType + ";base64," + companyInfo.base64minLogo;
            $('#imgFavicon .image-input-wrapper').css('background-image', 'url(' + faviconUrl + ')');
            $('#imgFavicon').attr('alt', companyInfo.companyDescription || 'Fevicon Logo');
        } else {
            $('#imgFavicon .image-input-wrapper').css('background-image', '');
        }

        // Set print logo
        if (companyInfo.base64printLogo) {
            var printLogoMimeType = getMimeType(companyInfo.base64printLogo);
            var printLogoUrl = "data:" + printLogoMimeType + ";base64," + companyInfo.base64printLogo;
            $('#imgPrintLogo .image-input-wrapper').css('background-image', 'url(' + printLogoUrl + ')');
            $('#imgPrintLogo').attr('alt', companyInfo.companyDescription || 'Print Logo');
        } else {
            $('#imgPrintLogo .image-input-wrapper').css('background-image', '');
        }
    }
}

function getMimeType(base64) {
    var prefixMap = {
        "PHN2Z": "image/svg+xml",
        "PD94b": "image/svg+xml",
        "77U": "image/svg+xml",
        "/9j/": "image/jpeg",        
        "iVBORw0KGgo": "image/png",
        "R0lGODdh": "image/gif",
        "PD94bWwgdmVyc2lvbj0iMS4wIiA/Pz4=": "image/svg+xml",
        "PHN2Zy": "image/svg+xml",
        "/9j/": "image/jpeg",           
        "Qk02": "image/jpeg"        
    };

    for (var prefix in prefixMap) {
        if (base64.startsWith(prefix)) {
            return prefixMap[prefix];
        }
    }

    return "image/png"; 
}


function SaveCompany(companyInfo) {
    var companyId = companyInfo.CompanyId;
    var companyCode = $('#txtCompCode').val();
    var companyDescription = $('#txtCompDesc').val();
    
    var addressid = $('#ddlCompanyAddress').val();

    var imgInput = document.querySelector('#imgLogoInput');
    var imgfaviconInput = document.querySelector('#imgFaviconInput');
    var imgprintInput = document.querySelector('#imgPrintLogoInput');

    var imageBase64 = '';
    var imagefaviconBase64 = '';
    var imageprintBase64 = '';

    var promises = [];

    if (imgInput.files[0]) {
        promises.push(new Promise(function (resolve) {
            convertImageToBase64(imgInput.files[0], function (base64Data) {
                imageBase64 = base64Data;
                resolve();
            });
        }));
    } else {
        imageBase64 = ''; 
    }

   
    if (imgprintInput.files[0]) {
        promises.push(new Promise(function (resolve) {
            convertImageToBase64(imgprintInput.files[0], function (base64Data) {
                imageprintBase64 = base64Data;
                resolve();
            });
        }));
    } else {
        imageprintBase64 = ''; 
    }

    if (imgfaviconInput.files[0]) {
        promises.push(new Promise(function (resolve) {
            convertImageToBase64(imgfaviconInput.files[0], function (base64Data) {
                imagefaviconBase64 = base64Data;
                resolve();
            });
        }));
    } else {
        imagefaviconBase64 = '';
    }


    Promise.all(promises).then(function () {
        var CompanyUpdateModal = {
            companydetails: {
                companyid: parseInt(companyId),
                companydescription: companyDescription,
                companycode: companyCode,
                addressid: addressid,
                mainlogo: imageBase64,
                mainlogofilename: imgInput.files[0] ? imgInput.files[0].name : '',
                printlogo: imageprintBase64,
                printlogofilename: imgprintInput.files[0] ? imgprintInput.files[0].name : '',
                faviconlogo: imagefaviconBase64,
                faviconlogofilename: imgfaviconInput.files[0] ? imgfaviconInput.files[0].name : ''
            },
            accessuserid: parseInt(userId)
        };


        $.ajax({
            url: pathname+'/UserAdministration/UpdateCompanyDetails',
            type: 'POST',
            data: { data: JSON.stringify(CompanyUpdateModal) },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Company details updated successfully!',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Update Failed',
                        text: 'Failed to update company details: ' + response.message,
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error('Error saving company details:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred while updating company details.',
                    confirmButtonText: 'OK'
                });
            }
        });
    });
}

function convertImageToBase64(file, callback) {
    var reader = new FileReader();
    reader.onload = function (e) {
        callback(e.target.result.split(',')[1]);
    };
    reader.onerror = function (error) {
        console.error('Error converting image to base64:', error);
        callback(null);
    };
    reader.readAsDataURL(file);
}

function handleCancelAction(imageId) {
    var $imageInputWrapper = $('#' + imageId + ' .image-input-wrapper');
    $imageInputWrapper.css('background-image', ''); 
    $('#' + imageId + ' input[type="file"]').val(''); 
    $('#' + imageId + ' input[type="hidden"]').val(''); 
}

function handleRemoveAction(imageId) {
    handleCancelAction(imageId);
}

$('[data-kt-image-input-action="cancel"]').on('click', function () {
    var imageId = $(this).closest('.image-input').attr('id');
    handleCancelAction(imageId);
});

$('[data-kt-image-input-action="remove"]').on('click', function () {
    var imageId = $(this).closest('.image-input').attr('id');
    handleRemoveAction(imageId);
});

