var tbl = null;

$(document).ready(function () {
    $('#nav_userAdmin').addClass("show");
    $('#menuUser .menu-link').addClass("active");

    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });

    InitialiseTable(pathname + "/UserAdministration/GetUserList");

   

    //$('#view-details').click(function (e) {

    //    var userData = document.getAttribute('data-id');
    //    window.location.href = pathname + "/UserAdministration/UserDetail?userId=" + userId;
    //})
    //$('#btnAddNewUser').click(function (e) {
    //    console.log('inside button event');
    //    window.location.href = pathname + '/UserAdministration/CreateNewUser';
    //});

    //$('#btnAddnew').click(function (e) {
    //    console.log('inside button event');
    //    window.open(pathname + '/UserAdministration/CreateNewUser', '_blank');
    //});
    
});

//$(document).addEventListener('DOMContentLoaded', function () {
//    var viewDetailsElements = document.querySelectorAll('.view-details');
//    viewDetailsElements.forEach(function (element) {
//        element.addEventListener('click', function () {
//            var userId = element.getAttribute('data-id');
//            window.location.href = '/YourController/YourAction?userId=' + userId;
//        });
//    });
//});



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
            "serverSide": true,
            "datatype": "json",
            "dataSrc": function (data) {
                totalrecords = data.recordsTotal;
                return data.data;
            }
        },
        "columnDefs": [
            { "visible": false, "targets": 6 },
            { "visible": false, "targets": 0 }
        ],
        "columns": [
            { "data": "ex_userid", "name": "", "autoWidth": true },
            {
                "data": "ex_userid", "name": "User Code", "autoWidth": true, "render": function (data, type, full, meta) {
                    // Function to determine background color based on the first letter of the user code
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

                    var avatarColorClass = getBackgroundColor(full.ex_usercode.charAt(0));
                    var avatarHtml = '<div class="symbol symbol-circle symbol-50px overflow-hidden me-3 view-details" data-id="' + full.ex_userid + '" style="cursor: pointer;">' +
                        '<div class="symbol-label"><div class="symbol-label fs-3 ' + avatarColorClass + '">' + full.ex_usercode.charAt(0) + '</div></div>' +
                        '</div>';
                    var userDetailsHtml = '<div class="d-flex flex-column ms-3 view-details" data-id="' + full.ex_userid + '" style="cursor: pointer;">' +
                        '<span class="text-gray-800 text-hover-primary mb-1 fw-bolder fs-6">' + full.ex_usercode + '</span>' +
                        '<span class="text-gray-600 fw-bold fs-6">' + full.ex_emailid + '</span></div>';
                    return '<div class="ps-5 d-flex align-items-center sorting_1">' + avatarHtml + userDetailsHtml + '</div>';
                }
            },

            { "data": "ex_username", "name": "User Name", "autoWidth": true },
            { "data": "usertypedescr", "name": "Role", "autoWidth": true },
            {
                "data": "isactive", "name": "Status", "autoWidth": true, "render": function (data, type, full, meta) {
                    var status = data == 1 ? 'Active' : 'Inactive';
                    var badgeClass = data == 1 ? 'badge-light-success' : 'badge-light-danger';
                    return '<div class="badge ' + badgeClass + ' fw-bolder">' + status + '</div>';
                }
            },
            { "data": "company_description", "name": "Company", "autoWidth": true },
            {
                "data": "pwd_expired", "name": "Password Expiry Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (data != null) {
                        return moment(data).format('DD/MM/YYYY');
                    } else {
                        return "";
                    }
                }
            }
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
    SearchData(oTable);
}





$(document).on('click', '.view-details', function () {
    
    var userId = $(this).data('id');
    ViewDetails(userId);
});

function ViewDetails(userid) {
    console.log(userid)
    $.ajax({
        url: pathname + '/Outbound/EncryptId',
        type: 'GET',
        data: { id: userid },
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

/*function InitialiseTable(_url) {
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
        *//*"scrollY": '50vh',
        "scrollCollapse": true,
        "scrollX": true,*//*
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
            "serverSide":true,
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var sd = 0;
            },

            "dataSrc": function (data) {
                totalrecords = data.recordsTotal;
                return data.data;
            }
        },

        "columnDefs": [
            
        ],
        "columns": [
            {
                "data": "ex_userid", "name": "User Code", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        return "<a href='#'  data-id=" + full.userid + " onclick='ViewDetails(" + full.ex_userid + ")'>" + full.ex_usercode + "</a>"

                    }
                    else {
                        return full.ex_usercode;
                    }
                }
            },
            { "data": "ex_username", "name": "User Name", "autoWidth": true },
            { "data": "usertypedescr", "name": "Role", "autoWidth": true },
            { "data": "isactive", "name": "Status", "autoWidth": true },
            { "data": "companyid", "name": "Company", "autoWidth": true },
            {
                "data": "pwd_expired", "name": "Password Expiry Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (data != null) {
                        return moment(data).format('DD/MM/YYYY HH:mm:ss');
                    } else {
                        return "";
                    }
                }
            },
           
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
    SearchData(oTable);
};
*/