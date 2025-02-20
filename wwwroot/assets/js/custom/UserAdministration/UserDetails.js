

var oDocTextTable = null; var docId = 0; var lstValidForSite = null;
var isEdit = 0;

$(document).ready(function () {
    $('#nav_userAdmin').addClass("show");
    $('#menuUser .menu-link').addClass("active");
    _detailpageLoaded = true;
    $('#txtroledrop').select2({
        width: '100%',
        allowClear:true,
        dropdownParent: "#addCompanyModel"
    });
   

    $('#btnSaveUserDetail').on("click", function () {
        Swal.fire({
            title: "Saving User Details",
            text: "Are you sure want to save the changes?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: "Yes! Proceed",
            cancelButtonText: "No, cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                SaveUser();
            }
        });
    });

    $('#btnAddLinkedCompany').on('click', function (event) {
        event.preventDefault(); // Prevent the default link behavior
        PopulateCompaniesForLinking();
        $('#addCompanyModel').modal('hide');
    });

    PopulateFormDetails();

    
});

$(document).on('click', '#btnRemoveCompany', function (e) {
    e.preventDefault();

    // Get the data attributes
    var userId = $(this).data('userid');
    var companyId = $(this).data('companyid');
   
    Swal.fire({
        title: "Proceeding!",
        text: "Are you sure want to remove the company?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, Proceed",
        cancelButtonText: "No, cancel"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: pathname + "/UserAdministration/RemoveLinkedCompany",
                type: "POST",
                data: {
                    "userId": userId,
                    "companyId": companyId
                },
                success: function (response) {
                    if (response.isSuccess) {
                        Swal.fire({
                            title: 'Success!',
                            text: 'Company link removed successfully.',
                            icon: 'success',
                            showCancelButton: false,
                            confirmButtonText: 'Ok'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.reload();
                            }
                        })
                       
                    }
                    else {
                        Swal.fire('Error', response.message, 'error')
                    }
                }
                
            });

        }
    });

   
});

function PopulateCompaniesForLinking() {
    var dropdown = $('#txtcompanydrop');
    var rolesDropdown = $('#txtroledrop');

    dropdown.empty();
    rolesDropdown.empty();

    dropdown.append('<option value="" disabled selected>Select Company</option>');
    rolesDropdown.append('<option value="" disabled selected>Select Role</option>');

    companies.forEach(function (company) {
        var option = `<option value="${company.companyid}">${company.company_description}</option>`;
        dropdown.append(option);
    });

    dropdown.select2();

    roles.forEach(function (role) {
        var optionRole = `<option value="${role.usertypeid}">${role.usertypedescr}</option>`;
        rolesDropdown.append(optionRole);
    });

    rolesDropdown.select2();

    var selectedCompanyValue = "";
    var selectedRoleValue = "";

    dropdown.on('change', function () {
        selectedCompanyValue = $(this).find('option:selected').val();
    });

    rolesDropdown.on('change', function () {
        selectedRoleValue = $(this).find('option:selected').val();
    });

    $('#btnAddCompany').off('click').on('click', function () {
        if (selectedCompanyValue === "" && selectedRoleValue === "") {
            Swal.fire('Error', 'Please select both company and role.', 'error');
            return;
        }
        addLinkCompany(selectedCompanyValue, selectedRoleValue, userdetails.ex_userid);
    });
    $('#txtcompanydrop').select2({
        width: '100%',
        allowClear: true,
        dropdownParent: "#addCompanyModel"
    });
    $('#txtroledrop').select2({
        width: '100%',
        allowClear: true,
        dropdownParent: "#addCompanyModel"
    });
    $('#addCompanyModel').modal('show');
}

function addLinkCompany(CompanyId, RoleId, UserId) {
    $.ajax({
        url: pathname + '/UserAdministration/AddLinkCompany',
        type: 'POST',
        data: { 'CompanyId': CompanyId, 'RoleId': RoleId, 'UserId': UserId },
        success: function (data) {
            if (data.isSuccess) {
                Swal.fire({
                    title: 'Success!',
                    text: 'Changes Saved successfully.',
                    icon: 'success',
                    showCancelButton: false,
                    confirmButtonText: 'Ok'
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.reload();
                    }
                })
            }
            else {
                Swal.fire('Error', data.data.message, 'error');
            }
        }
        
        
    });
}

function PopulateFormDetails() {
    var dropdown = $('#txtcompany');
    var rolesDropdown = $('#txtrole');

    var defaultCompany = userdetails.company_code;
    var defaultUserType = userdetails.usertype;

    var previousCompanyValue = dropdown.val();

    var roleSelected = $('#txtrole').find('option:selected').text();

    companies.forEach(function (company) {
        var selected = company.company_code === defaultCompany ? 'selected' : '';
        var option = `<option value="${company.companyid}" ${selected}>${company.company_description}</option>`;
        dropdown.append(option);
    });

    roles.forEach(function (role) {
        var selectedRole = role.usertypeid === defaultUserType ? 'selected' : '';
        var optionRole = `<option value="${role.usertypeid}" ${selectedRole}>${role.usertypedescr}</option>`;
        rolesDropdown.append(optionRole);
    });

    dropdown.select2();
    rolesDropdown.select2();

    

    dropdown.on('focus', function () {
        previousCompanyValue = dropdown.val();
    });

    dropdown.on('change', function () {
        var selectedCompany = $(this).find('option:selected').text();
        var selectedCompanyid = $(this).find('option:selected').val();
        if (selectedCompanyid !== userdetails.companyid) {
            Swal.fire({
                title: 'Company Selected',
                text: 'you want to switch default company to ' + selectedCompany + '?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, proceed',
                cancelButtonText: 'No, cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                        SaveUser()

                } 
                else {

                    dropdown.val(userdetails.companyid).trigger('change');
                }
            });
        }
        
        
    });

    SetActiveTabs();
    InitializeTable(linkedcompanies);
}

function SetActiveTabs() {
    if (userdetails.isactive == 1) {
        document.getElementById("chkUserActive").setAttribute("checked", "checked");
    } 
    if (userdetails.mail_notification == 1) {
        document.getElementById("chkNotiifications").setAttribute("checked", "checked");
    }
}



function InitializeTable(linkedCompies) {
    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
    if (slModuleAction[7] > 2) {
        isaccess = true;
    }
    $("#tblCompanies").DataTable({
        "language": {
            "infoFiltered": ""
        },
        "scrollY": '15vh',
        info: false,
        ordering: false,
        paging: true,
        //"scrollCollapse": true,
        //"scrollX": true,
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
        "data": linkedCompies,
        "columnDefs": [
            { "visible": isaccess, "targets": 2 }
        ],
        "columns": [
            /*{ "data": "user_company_link_id", "name": "", "autoWidth": true },*/

            { "data": "company_description", "name": "Company", "autoWidth": true },
            { "data": "usertypedescr", "name": "Role", "autoWidth": true },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (userdetails.companyid == full.companyid) {
                        return '<a class="btn btn-sm btn-link btn-color-danger me-5 btnremoveattach" data-userId ="' + full.ex_userid + '" data-companyId ="' + full.companyid + '" data-filename="' + full.documentName + '" href="#" style="color:#fa7d7d;pointer-events: none;">Remove</a>'
                    } else {
                        return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnremoveattach" id="btnRemoveCompany" data-userId ="' + full.ex_userid + '" data-companyId ="' + full.companyid + '" data-filename="' + full.documentName + '" href="#" >Remove</a>'
                    }
            }
            }
        ],
        "language": {
            "emptyTable": 'No Company Linked Yet!'
        },

    });
}
function SaveUser() {
    var IsSave = true;
    var active = $('#chkUserActive').is(':checked')
    var mailnotification = $('#chkNotiifications').is(':checked')
    var formData = {
        UserDetails: {
            ex_userid: userdetails.ex_userid,
            ex_usercode: $('#txtusername').val(),
            ex_username: $('#txtfirstname').val(),
            created_date: convertToDate($('#createddt').val()),
            ex_emailid: $('#txtEmail').val(),
            //companyid: $('#txtcompany').val(),
            //usertype: $('#txtrole').val(),
            password_expiry_date: $('#txtPwdExpiry').val(),
            isactive: (active === true) ? 1 : 0,
            mail_notification: (mailnotification === true) ? 1 : 0,
            usertype: $('#txtrole option:selected').val(),
            companyid: $('#txtcompany option:selected').val()
        },
        Password: $('#txtpassword').val(),
        ConfirmPassword: $('#txtconfirmpassword').val()      
    };



    //console.log(formData);
    if (formData.Password != userdetails.ex_password) {
        if (formData.Password != formData.ConfirmPassword) {
            Swal.fire('Error', 'Password Does Not Match', 'error');
            IsSave = false;
        }
    }
    if (IsSave) {
        $.ajax({
            url: pathname + '/UserAdministration/SaveUser',
            type: 'POST',

            data: { "userData": JSON.stringify(formData) },
            success: function (data) {
                if (data.result) {
                    Swal.fire({
                        title: 'Success',
                        text: 'Details updated Successfully!',
                        icon: 'success',
                        showCancelButton: false,
                        confirmButtonText: 'Ok'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.reload();
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
//function InitializeValidForSite() {
//    var _url = pathname + "/UserAdministration/GetValidSites";
//    ClearSiteForm();
//    $("#tblValidForSite").DataTable().clear().draw().destroy();
//    oDocTextTable = $("#tblValidForSite").DataTable({
//        "language": {
//            "infoFiltered": ""
//        },
//        "scrollY": '40vh',
//        "scrollCollapse": true,
//        "scrollX": true,
//        "processing": true,
//        "serverSide": false,
//        "filter": true,
//        "orderMulti": false,
//        "lengthMenu": [
//            [10, 25, 50, -1],
//            ['10 rows', '25 rows', '50 rows', 'Show all']
//        ],
//        "columnDefs":
//            [{
//                "targets": [0],
//                "visible": false,
//                "searchable": false,
//                "defaultContent": "0"
//            }],

//        "columns": [

//            { "data": "siteId", "name": "", "autoWidth": true },
//            { "data": "site_Code", "name": "Site Code", "autoWidth": true },
//            { "data": "site_Name", "name": "Site Description", "autoWidth": true },
//            {
//                "data": "null", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
//                    // return '<td class="min-w-20px" style="width:10%"><div id="divEdit' + full.doc_Txt_ID + '"><a class="btn btn-sm btn-link btn-color-primary btn-active-color-primary me-5 btnEdit" onclick="EditRow(this)" href="#">Edit</a><a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 " href="#" onclick="DeleteRow(this)"></a></div><div class="hide" id="divSave' + full.doc_Txt_ID + '"> <a class="btn btn-sm btn-link btn-color-success btn-active-color-success me-5 " href="#" onclick="SaveRow()">Save</a><a class="btn btn-sm btn-link btn-color-gray-500 btn-active-color-gray-500 me-5 " href="#" onclick="CancelEdit()">Cancel</a></div> </td>';
//                    return '<td id="divReomve' + full.siteId + '" class="min-w-20px" style="width:10%"><div class="removeclick" ><a id="divremove' + full.siteId + '" class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnremove" href="#">Remove</a></div></td>';
//                }
//            }
//        ],
//        "initComplete": function (data) {
//            RemoveClick();
//            $('#aSave').click(function (e) {
//                //SaveRows();
//            });

//            $('#aCancel').click(function (e) {
//                //CancelEdit();
//            });
//        },
//        "drawCallback": function () {
//            RemoveClick();
//            //EditbtnClick();
//        },
//        "pageLength": 10,
//        "bDestroy": true,
//        "ajax": {
//            "url": _url,
//            "data": { USERID: user.eX_USERID },
//            "type": "POST",
//            "datatype": "json",
//            "error": function (XMLHttpRequest, textStatus, errorThrown) {
//            },
//            "dataSrc": function (data) {
//                lstValidForSite = data.data;
//                $("#TotalRecord").text(data.data.length);
//                return data.data;
//            }
//        },
//    });

//    $('.itemaction').click(function (e) {

//        LinkSite(e); 
//    })
//    $('#btnAddNewItem').show();
//    $('#tblValidForSite').on('order.dt', function () {
//        //console.log('tblValidForSite sort event handler called.');
//        $('#btnAddNewItem').show();
//        $('#aSave').click(function (e) {
//            //SaveRows();
//        });

//        $('#aCancel').click(function (e) {
//            //CancelEdit();
//        });
//        // Your code to handle the sort event goes here
//    });
//    docId = 0;
//    isEdit = 0;
   
//}
//function RemoveClick() {
//    $('.btn-active-color-danger').unbind().click(function (e) {
//        var Siteid = $(this).attr('id').replace('divremove', '');
//        if (Siteid != '') {
//            RemoveLink(Int(Siteid));
//        }

//    });

//}
//function LinkSite(e) {
//    ClearSiteForm();
//    var table = $("#tblValidForSite").DataTable();
//    var data = table
//        .rows()
//        .data();
//    console.log('on Add: ' + data.length);
    
//    var ComOpts = '<option></option>';
//    for (var i = 0; i < AllSites.length; i++) {
//        var objs = AllSites[i];
//        var SiteName = objs.site_Code + ' - ' + objs.site_Name
//        ComOpts += '<option value="' + objs.siteId + '">' + objs.site_Code + '</option>';
//    }

    
//    //var ExtraRow = [
//    //    '<input type="text" class="form-control fs-7" id="txtSiteNames" maxlength="250" placeHolder="Site Description" readonly/>',
//    //    '<input type="text" class="form-control fs-7" id="txtSiteName" maxlength="250" placeHolder="Site Description" readonly/>',
//    //    '<div id="divSave"><a class="btn btn-sm btn-link btn-color-success btn-active-color-success me-5 mb-2" href="#" id="aSave">Save</a><a class="btn btn-sm btn-link btn-color-gray-500 btn-active-color-gray-500 me-5 mb-2" href="#" id="aCancel">Cancel</a></div>'];
//    //oDocTextTable.row.add(ExtraRow).draw();
//    //$(e).addClass('show');

//    var shtml = '<tr id="dynamicRow">'
//    shtml += '<td><select id="selSite" class="form-select" data-placeholder="Select Sites" >' + ComOpts + '</select></td>';
//    shtml += '<td><input type="text" class="form-control fs-7" id="txtSiteName" maxlength="250" placeHolder="Site Description" readonly/></td>';
//    shtml += '<td><div id="divSave"><a class="btn btn-sm btn-link btn-color-success btn-active-color-success me-5 mb-2" href="#" id="aSave">Save</a><a class="btn btn-sm btn-link btn-color-gray-500 btn-active-color-gray-500 me-5 mb-2" href="#" id="aCancel">Cancel</a></div></td></tr>';
//    $(e).addClass('show');
  
//    $('#tblValidForSite tbody').prepend(shtml);
//    $('#selSite').select2();
//    $('#btnAddNewItem').hide();
    
//    $('#aSave').click(function (e) {
//        SaveSite();
//    });
//    $('#aCancel').click(function (e) {
//        CancelSiteEdit();
//    });
//    $('#selSite').on('change', function (e) {
//        $('#txtSiteName').val('');
//        var Siteid = $('#selSite option:selected').val();
//        if (Siteid != "") {
//            var siteName = Enumerable.from(AllSites).where(x => x.siteId == Siteid).toArray()[0].site_Name;
//            $('#txtSiteName').val(siteName);
//        }
        
//    });
//}
//function RemoveLink(siteid) {
//    Swal.fire({
//        title: '', text: "Are you sure you want to Remove Site From User ?'",
//        showCancelButton: true, confirmButtonText: 'Yes',
//        cancelButtonText: 'No',
//    }).then((result) => {
//        if (result.isConfirmed) {
//            if (siteid != null || siteid > 0) {
//                $.ajax({
//                    type: "POST", async: false,
//                    url: pathname + "/UserAdministration/RemoveSiteLink",
//                    data: { "USERID": user.eX_USERID, "SITEID": siteid },
//                    success: function (response) {

//                        if (response.result == true) {
//                            var msg = 'Site Removed successfully!';

//                            Swal.fire({
//                                text: msg,
//                                icon: "success",
//                                buttonsStyling: !1,
//                                confirmButtonText: "Ok",
//                                customClass: { confirmButton: "btn btn-primary" },
//                                showClass: {
//                                    popup: 'animate__animated animate__fadeInDown'
//                                },
//                                hideClass: {
//                                    popup: 'animate__animated animate__fadeOutUp'
//                                }
//                            }).then((function (e) {
//                                InitializeValidForSite();
                                
//                            }));

//                        }
//                        else {
//                            if (response.msg != '') {
//                                Swal.fire('Warning', response.msg, 'warning').then((function (e) {
//                                    $('#btnAddnew').show();
                                   
//                                }));
//                            }
//                            else {
//                                Swal.fire('Error', 'Unable to Remove Site', 'error').then((function (e) {

                                    
//                                }));
//                            }

//                        }

//                    },
//                    error: function (response) {

//                        if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
//                            Swal.fire('Unable to Remove Site', "Please enter unique code", 'error');
//                        }
//                        else Swal.fire('Unable to Remove Site', response.responseText, 'error');
//                    }
//                });
//            }
//        }
//    });


//}
//function CancelSiteEdit(e) {

//    //$('#tblValidForSite').on('click', '.dynamic-row .aCancel', function (e) {
//    //    e.preventDefault();
//    //    $(this).closest('tr').remove();
//    //});
//    //location.reload();
//    InitializeValidForSite();
    
//    $('#btnAddNewItem').show();

    
//    isEdit = 0;


//}
//function SaveSite() {
//    var siteid = $('#selSite option:selected').val();
//    if (siteid != null || siteid > 0) {
//        $.ajax({
//            type: "POST", async: false,
//            url: pathname + "/UserAdministration/SaveUserSiteLink",
//            data: { "USERID": user.eX_USERID, "SITEID": siteid},
//            success: function (response) {
//                try {
//                    if (response.result == true) {
//                        var msg = 'Site added successfully!';

//                        Swal.fire({
//                            text: msg,
//                            icon: "success",
//                            buttonsStyling: !1,
//                            confirmButtonText: "Ok",
//                            customClass: { confirmButton: "btn btn-primary" },
//                            showClass: {
//                                popup: 'animate__animated animate__fadeInDown'
//                            },
//                            hideClass: {
//                                popup: 'animate__animated animate__fadeOutUp'
//                            }
//                        }).then((function (e) {
//                            $('#btnAddnew').show();
//                            InitializeValidForSite();
//                            //location.reload();
//                        }));

//                    }
//                    else {
//                        if (response.msg != '') {
//                            Swal.fire('Warning', response.msg, 'warning').then((function (e) {
//                                $('#btnAddnew').show();
//                                InitializeValidForSite();
//                            }));
//                        }
//                        else {
//                            Swal.fire('Error', 'Unable to Add Site', 'error').then((function (e) {
//                                $('#btnAddnew').show();
//                                InitializeValidForSite();
//                            }));
//                        }

//                    }
//                }
//                catch (err) {
//                    Swal.fire('Error', 'Unable to Add Site', 'error').then((function (e) {
//                        $('#btnAddnew').show();
//                        InitializeValidForSite();
//                    }));
//                }
//            },
//            error: function (response) {

//                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
//                    Swal.fire('Unable to Add Site', "Please enter unique code", 'error');
//                }
//                else Swal.fire('Unable to Add Site', response.responseText, 'error');
//            }
//        });
//    }
//}





