var addressid = 0;
$(document).ready(function () {
    
    $('#nav_userAdmin').addClass("show");
    $('#menuCompany .menu-link').addClass("active");
    $('#ddlCompanyAddress').select2({
        dropdownAutoWidth: true,
        allowClear: true,
        dropdownParent: $('#addCompanyModal')
    });
    var addressid = window.addressId;
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });

    InitialiseTable(pathname + "/UserAdministration/GetCompanyList");
    loadAddressDropdown();
    $('#ddlCompanyAddress').on('change', function () {
        var addressid = $(this).val();
        loadSMAddressDetails(addressid);
    });

    $('#btnSaveChanges').on("click", function () {
        SaveCompany();
    });
    

    
});


function InitialiseTable(_url) {
    var isaccess = false;
    var totalrecords = 0;
    if (slModuleAction[7] > AccessLevels.Write) {
        isaccess = true;
    }
    var rdVal = Math.floor(Math.random() * 100) + 1;

    oTable = $("#tblData").DataTable({
        "language": {
            "infoFiltered": ""
        },
        "processing": true,
        "serverSide": false,
        "filter": true,
        "orderMulti": false,
        "order": [[0, "desc"]],
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10,
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { ValueRD: rdVal },
            "type": "POST",
            "datatype": "json",
            "dataSrc": function (data) {
                totalrecords = data.recordsTotal;
                return data.data;
            },
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                console.error("Error fetching data from API:", errorThrown);
            }
        },
        "columns": [
            {
                "data": "companyid", "name": "Company Id", "autoWidth": true, "render": function (data, type, full, meta) {
                    function getBackgroundColor(letter) {
                        switch (letter.toUpperCase()) {
                            case 'A': case 'B': case 'C': return 'bg-light-primary text-primary';
                            case 'D': case 'E': case 'F': return 'bg-light-success text-success';
                            case 'G': case 'H': case 'I': return 'bg-light-info text-info';
                            case 'J': case 'K': case 'L': return 'bg-light-warning text-warning';
                            case 'M': case 'N': case 'O': return 'bg-light-danger text-danger';
                            case 'P': case 'Q': case 'R': return 'bg-light-dark text-dark';
                            case 'S': case 'T': case 'U': return 'bg-light-primary text-primary';
                            case 'V': case 'W': case 'X': return 'bg-light-success text-success';
                            case 'Y': case 'Z': return 'bg-light-info text-info';
                            default: return 'bg-light-secondary text-secondary';
                        }
                    }

                    var avatarColorClass = getBackgroundColor(full.company_code.charAt(0));
                    var avatarHtml = '<div class="symbol symbol-circle symbol-50px overflow-hidden me-3" style="cursor: pointer;" onclick="ViewDetails(' + full.companyid + ')">' +
                        '<div class="symbol-label"><div class="symbol-label fs-3 ' + avatarColorClass + '">' + full.company_code.charAt(0) + '</div></div>' +
                        '</div>';
                    var userDetailsHtml = '<div class="d-flex flex-column ms-3" style="cursor: pointer;" onclick="ViewDetails(' + full.companyid + ')">' +
                        '<span class="text-gray-800 text-hover-primary mb-1 fw-bolder fs-6">' + full.company_code + " - " + full.company_description + '</span>';
                    return '<div class="ps-5 d-flex align-items-center sorting_1">' + avatarHtml + userDetailsHtml + '</div>';
                }
            },
            { "data": "addr_type", "name": "Type", "autoWidth": true },
            { "data": "country", "name": "Country", "autoWidth": true },
        ],
        "initComplete": function (data) {
            $('#lblTotalRecord').text(totalrecords);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        }
    });
}

function ViewDetails(_id) {
    $.ajax({
        url: pathname + '/Outbound/EncryptId',
        type: 'GET',
        data: { id: _id },
        
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/UserAdministration/CompanyDetails?Companyid=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
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
             

            } else {
                console.error('No details found for the selected address.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading address details:', error);
        }
    });
}
function loadAddressDropdown() {
    $.ajax({
        url: pathname+'/UserAdministration/GetAllSMAddress',
        type: 'GET',
        data: { },
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
            dropdown.select2({
                dropdownParent: $('#addCompanyModal'),
                placeholder: "Select an address", // Optional placeholder
                allowClear: true // Optional: to allow clearing the selection
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading address data:', error);
        }
    });
}

function SaveCompany() {
    var companyData = {
        companyid: 0,  
        companydescription: $('#txtCompDesc').val(),
        companycode: $('#txtCompCode').val(),
        addressid: parseInt($('#ddlCompanyAddress').val()),  
        mainlogo: '', 
        mainlogofilename: '',
        printlogo: '',
        printlogofilename: '',
        feviconlogo: '',
        feviconlogofilename: ''
    };

    $.ajax({
        url: pathname+'/UserAdministration/CreateNewCompany',
        type: 'POST',
        data: { data: JSON.stringify(companyData) },
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: 'Company details added successfully!',
                    confirmButtonText: 'OK'
                }).then(() => {
                    window.location.reload();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Update Failed',
                    text: 'Failed to add company details: ' + response.message,
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
}
function AddNew() {
    var myModal = new bootstrap.Modal(document.getElementById('addCompanyModal'));
    myModal.show();
}