var userName = ''; var userId = 0; var userType = ''; var datefrom; var dateto; var labelsupp = []; var menuName = ''; var reportType = ''; var selectedtab = '';
var userCode = ''; var buyerContacts = ''; var supplierId = 0; var reporttype_datename = ''; var isAsc = ''; var _filterUserType = '', _filterUserTypeId = '',
    _selectedUserId = '',  _selectedModAccess = '';
var OrderbyText = ''; var OrderByVal = ''; var sortType = ''; var companyId = 0; var userActive = 0; var opMode = ""; var oTable = ''; var IsValidUserEmail = true;
var _filterTags = []; var userTypeFilter = ''; var ModuleNameFilter = ''; var ActionNameFilter = ''; var UseraccessList = [];
var tblUsersLists;
var oUsertypeEDITTable = null;
var oUsertypeAddable = null;
function clear() {
    userName = ''; menuName = ''; reportType = ''; userCode = ''; userType = ''; opMode = ""; userTypeFilter = ""; _filterUserType = ''; _filterUserTypeId = '';
    _filterTags = []; _selectedUserId = '';
}

$(document).ready(function () {

    $('#divTableTools').remove();
    $(".me-0").remove();
    $('#nav_UserAdmin').addClass("show");
    Initialize();
    LoadUserMaster();
    oTable = $("#tblUserTypeModuleAccess").dataTable();
    $("#ddlUsertype").on("change", function () {
        var Usertypeid = $('#ddlUsertype option:selected').val();
        GetUserTypeID(Usertypeid);
       
        
        
            
    });
    $('#btneditusertype').on('click', function () {
        var Usertypeid = $('#ddlUsertype option:selected').val();
        $('#modalItems').modal('show');
        GetUserTypeIDEDIT(Usertypeid);
    })
    $('#btnAddSaveUserType').on('click', function () {
        var Usertypeid = "0";
        var selectedOptions = [];


        $('[id^="newddlaccess"]').each(function () {

            var moduleId = this.id.split("_")[1];
            
            var selectedValue = $(this).val();
            selectedOptions.push({
                moduleId: moduleId,
                selectedValue: selectedValue
            });
            
        });
        SaveUserTypeDetails(selectedOptions, Usertypeid)
        
    })
    $('#btnSaveUserTypeInfo').on('click', function () {
        var Usertypeid = $('#ddlUsertype option:selected').val();
        var selectedOptions = [];

        
        $('[id^="ddlaccess"]').each(function () {
            
            var moduleId = this.id.split("_")[1];
            var selectedValue = $(this).val();
            selectedOptions.push({
                moduleId: moduleId,
                selectedValue: selectedValue
            });
        });
        SaveUserTypeDetails(selectedOptions, Usertypeid)

    })
    
});

function Initialize() {

    //$('#nav_useradministration').addClass('show');
    $('#nav_useradministration .menu-link').addClass('active');

    $('#btnNew').show();
    InitializeTable();
    $('#kt_drawer_filter_toggle').show();
    $('#selUserType').select2(); $('#selFtrUserType').select2(); $('#selCopyFrom').select2(); $('#selUserActive').select2();
    $("#selUserActive,#selUserType,#selBuyerCompany").select2({ dropdownParent: "#modal_detail" });
    $("#selCopyFrom").select2({ dropdownParent: "#modal_Usertypedetail" });
    $('#kt_tabPane_Users').addClass("show");
    
    selectedtab = $(".nav li a.active").text();
   
   
    
};

function SaveUserTypeDetails(SelectedOptions, usertypeid) {
    var username = $('#AddUserType').val();
    if (usertypeid != null || usertypeid > 0) {
        $.ajax({
            type: "POST", async: false,
            url: pathname + "/UserAdministration/SaveUserTypeDetails",
            data: { "options": JSON.stringify(SelectedOptions), "usertypeid": usertypeid, "usertypename": username },
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
                            //$('#btnAddnew').show();
                            //$('#modalItems').modal('hide');
                            //$('#modalItemsADD').modal('hide');
                            
                            ////var Usertypeid = $('#ddlUsertype option:selected').val();
                            //GetUserTypeID(response.id);
                            window.open(pathname + "/UserAdministration/UsersAdmin", '_self');
                        }));

                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Warning', response.msg, 'warning').then((function (e) {
                                $('#btnAddnew').show();
                                
                            }));
                        }
                        else {
                            Swal.fire('Error', 'Unable to User type Details', 'error').then((function (e) {
                                $('#btnAddnew').show();
                                
                            }));
                        }

                    }
                }
                catch (err) {
                    Swal.fire('Error', 'Unable to save User type Details', 'error').then((function (e) {
                        $('#btnAddnew').show();
                        
                    }));
                }
            },
            error: function (response) {

                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                    Swal.fire('Unable to User type Details', "Please enter unique code", 'error');
                }
                else Swal.fire('Unable to User type Details', response.responseText, 'error');
            }
        });
    }
}
function InitializeTable() {
    tblUsersLists = $("#tblUsersLists").DataTable({
        "fixedHeader": { header: true },
        "processing": true, // for show progress bar
        "serverSide": false, // for process server side
        "filter": true, // this is for disable filter (search box)
        "orderMulti": false, // for disable multiple column at once
        "scrollY": '40vh',
        "scrollCollapse": true,
        "scrollX": true,
        "paging": true,
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],       
        "pageLength": 10,
        "bDestroy": true
    });
}

function HandleClicks(e) {
    var x = e.textContent;
    if (x == ' Add New User') {
        window.location = pathname + "/UserAdministration/AddUserDetails";
    }
    else if (x == ' Add User Type') {
        $('#modalItemsADD').modal('show');
        LoadUserTypeModuleADD();
    }
}

$("#selUserType").on("change", function (e) {
    userType = $("#selUserType option:selected");
});



//function SelectAccessValue(val) {
//    var result = '';
//    if (val == Str("No Access"))
//        result += '<option selected value=' + Str("0") + '>' + Str("No Access") + '</option>';
//    else
//        result += '<option value=' + Str("0") + '>' + Str("No Access") + '</option>';
//    if (val == Str("Full Access"))
//        result += '<option selected value=' + Str("3") + '>' + Str("Full Access") + '</option>';
//    else
//        result += '<option value=' + Str("3") + '>' + Str("Full Access") + '</option>';
//    if (val == '' || val == undefined || val == null)
//        result += '<option selected value=' + Str("0") + '>' + Str("No Access") + '</option>';

//    //if (val == Str("Read Only"))
//    //    result += '<option selected value=' + Str("1") + '>' + Str("Read Only") + '</option>';
//    //else
//    //    result += '<option value=' + Str("1") + '>' + Str("Read Only") + '</option>';
//    //if (val == Str("Add/Modify"))
//    //    result += '<option selected value=' + Str("2") + '>' + Str("Add/Modify") + '</option>';
//    //else
//    //    result += '<option value=' + Str("2") + '>' + Str("Add/Modify") + '</option>';
//    return result;
//};


//$('#btnSave').click(function () {   
//    saveRow();
//});


$('.nav-link').click(function (e) {
    $('.nav-link.active').removeClass('active');
    $('.tab-pane.show').removeClass('show');
    $(this).addClass('active');
    $($(this).attr('href')).addClass('show');
    selectedtab = $(this).text();
    $('#breadModule').text(selectedtab);//added by Kalpita on 19/01/2023
     ShowTabs();
});

function ShowTabs() {
    /*HideFilters();*/
    switch (selectedtab) {
        case 'Users': LoadUserMaster(); break;
        case 'User Types': LoadUserTypes(); break;
    }
};
function LoadUserTypes() {
    $('#divTableTools').remove();
    $('#btnadduser').html('<i class="bi la-cart-plus fs-3 me-2"></i> Add User Type');
    //document.getElementById("btnadduser").onclick = '/UserAdministration/';

}
//function HideFilters() {
//    if (selectedtab == 'UserAccess Rights') { $('#dvFilterUserType').show(); $('#dvFilterUser').hide();  }
//    else {
//        $('#dvFilterUser').show(); $('#dvFilterUserType').hide();
//    }
//};
function GetUserTypeID(Usertypeid) {
    $.ajax({
        type: "POST", async: false,
        url: pathname + "/UserAdministration/showDetails",
        data: { "id": Usertypeid },
        success: function (response) {
            LoadUserTypeModuleAccess(response.id);
            
        },
        error: function (response) {

        }
    });
}
function GetUserTypeIDEDIT(Usertypeid) {
    $.ajax({
        type: "POST", async: false,
        url: pathname + "/UserAdministration/showDetails",
        data: { "id": Usertypeid },
        success: function (response) {
            LoadUserTypeModuleEDIT(response.id);
            var selectElements = document.querySelectorAll("ddlaccess");

            // Loop through each select element and apply select2
            selectElements.forEach(function (selectElement) {
                $(selectElement).select2();
            });

        },
        error: function (response) {

        }
    });
}
//function clearFilter() {
//    $("#txtFltrUserCode").val('');
//    $("#txtFltrUserName").val('');
//    $("#selFtrUserType").val('').trigger('onchange');

//    clear();
//}

//function clearPopupFilter() {
//    $("#txtUserCode").val('');
//    $("#txtUserName").val('');
//    $("#txtUserEmail").val('');
//    $("#txtPassword").val('');
//    $("#txtConfPassword").val('');

//    $('#selUserActive').val('');
//    $('#selBuyerCompany').val('');
//    $('#selUserType').val('');
//}

//function CreateNewUser() {
//    clearPopupFilter();
//    opMode = "NEW";
//    $('#modal_detail').modal('show');
//}

//function getLastSegment() {
//    var url = window.location.pathname;
//    var array = url.split('/');
//    var lastsegment = array[array.length - 1];
//    console.log(lastsegment);
//    return lastsegment;
//};

//function ApplyFilter() {
//    $('.nav-link.active').click();
    
//    AddTagFilter();
//    $('span[data-role="remove"]').remove();
//};

//function AddTagFilter() {
//    $('.tagify').tagsinput({
//        allowDuplicates: true,
//        itemValue: 'id',  // this will be used to set id of tag
//        itemText: 'label' // this will be used to set text of tag
//    });
//    $('.tagify').tagsinput('removeAll');    
//    userCode = $('#txtFltrUserCode').val(); userName = $('#txtFltrUserName').val();

//    if (userCode != "") {
//        $('#UserCodetags').tagsinput('removeAll');
//        $('#UserCodetags').tagsinput('add', { id: 'tagsFrom', label: userCode });
//        var data = $('#UserCodetags').prev(); data.children().prepend("<b>UserCode : </b>");
//    }
//    else {
//        $('#UserCodetags').tagsinput('removeAll');
//    }
//    if (userName != "") {
//        $('#UserNametags').tagsinput('removeAll');
//        $('#UserNametags').tagsinput('add', { id: 'tagsFrom', label: userName });
//        var data = $('#UserNametags').prev(); data.children().prepend("<b>UserName : </b>");
//    }
//    else {
//        $('#UserNametags').tagsinput('removeAll');
//    }

//    if (_filterUserType != '') {
//        $("#UserTypetags").tagsinput('removeAll');
//        $('#UserTypetags').tagsinput('add', { id: 'tagsSupplier', label: _filterUserType });
//        var data = $('#UserTypetags').prev(); data.children().prepend("<b>UserType : </b>");
//    }
//    else {
//        $("#UserTypetags").tagsinput('removeAll');
//    }
//};

//$("#selUserType").on("change", function (e) {
//    userType = (this.value);
//    var theSelection = $("#selUserType option:selected").val();
//});

//$("#selFtrUserType").on("change", function (e) {
//    _filterUserType = $("#selFtrUserType option:selected").text();
//    _filterUserTypeId = $("#selFtrUserType option:selected").val();
//});

//$("#selCopyFrom").on("change", function (e) {
//    var copyFrom = $("#selCopyFrom option:selected");
//});

//$("#txtUserEmail").on("change", function (e) {
//    var inputvalues = $(this).val();
//    ValidateEmail(inputvalues);
   
//});

//function SetViewBag_TabValue(_selectedtab) {
//    var res = false;
//    $.ajax({
//        type: "POST",
//        async: false,
//        url: pathname + "/UserAdministration/SetViewBag",
//        data: "{'Tabvalue':'" + _selectedtab + "'}",
//        contentType: "application/json;charset=utf-8",
//        dataType: "json",
//        success: function (response) {
//        },
//        failure: function (response) {  },
//        error: function (response) {  }
//    });
//    return res;
//};

//function ValidateEmail(inputvalues) {  
//    if (inputvalues == '') return;
//    var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
//    if (!regex.test(inputvalues)) {
//        IsValidUserEmail = false;
//    }
//    else IsValidUserEmail = true;
//}


//User


function LoadUserMaster() {
    $("#btnadduser").show();
    $('#btnadduser').html('<i class="bi la-cart-plus fs-3 me-2"></i> Add New User');
    //document.getElementById("btnadduser").onclick = '/UserAdministration/AddUserDetails';
    var rdVal = Math.floor(Math.random() * 100) + 1;

    userName = $('#txtFltrUserName').val();
    userCode = $('#txtFltrUserCode').val();
    var jsondata = { 'UserCode': userCode, 'UserName': userName };
    $("#tblUsersLists").DataTable().clear();
    var tbl2 = $("#tblUsersLists").DataTable({
        "fixedHeader": {
            header: true,
        },
        "scrollY": '40vh',
        "scrollCollapse": true,
        "scrollX": true,
        "processing": true, // for show progress bar
        "serverSide": true, // for process server side
        "filter": true, // this is for disable filter (search box)
        "orderMulti": false, // for disable multiple column at once
        "lengthMenu": [
            [10, 25,  50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10,
        "bDestroy": true,
        "ajax": {
            "url": pathname + "/UserAdministration/FilterUsersData",
            "data": jsondata,
            "type": "POST",
            "datatype": "json",
            //"dataSrc": function (json) {
            //    var a = json;
            //},
            "error": function (XMLHttpRequest, textStatus, errorThrown) {

                
            }
        },  
        
        "columns": [
            {
                "data": "eX_USERCODE", "name": "User Code", "autoWidth": true, render: function (data, type, full, meta) {
                    return '<div data-userid=' + Str(full.eX_USERID) + ' data-doctype=UserDetail><a href="#" class="showmodal">' + full.eX_USERCODE + '</a ></div>';
                }
            },
            { "data": "eX_USERNAME", "name": "User Name", "autoWidth": true },
            { "data": "eX_EMAILID", "name": "Email Id", "autoWidth": true },
            { "data": "useR_STATUS", "name": "Active", "autoWidth": true },
            { "data": "usertypedescr", "name": "User Type", "autoWidth": true },
            {
                "data": "pwD_EXPIRY_DATE", "name": "Password Expiry Date", "autoWidth": true, render: function (data, type, full, meta) {
                    return moment(new Date(full.pwD_EXPIRY_DATE)).format('DD/MM/YYYY');
                }
            },
            {"data": "usertype", "name": "UserTypeID", "autoWidth": true, "sClass": "hide_column"},            
            { "data": "eX_PASSWORD", "name": "Password", "autoWidth": true, "sClass": "hide_column" }
        ],
        "drawCallback": function (settings) {
            $('.showmodal').on('click', function () {
                var _div = $(this).parent();
                var _doctype = $(_div).attr('data-doctype');
                var _userid = $(_div).attr('data-userid');
                var _currentRow = $(this).closest("tr");
                opMode = "UPDATE";
                if (_userid !==null) {
                    $.ajax({
                        type: "POST", async: true,
                        url: pathname + "/UserAdministration/showDetails",
                        data: { "id":_userid },
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
                }
            });
        },
        "initComplete": function (data) {
            $('#lblTotalRecord').text(data._iRecordsTotal);
        }
    });
    setTimeout(function () {
            $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
        }, 200);
        $('#kt_drawer_filter_close').click();
        $('.dataTable thead tr th').removeClass('text-end');
    };

//function ShowUserDetail(_userid, _currentRow) {
//    try {
//        var pCompany = '', pUserCode = '', pUserName = '', pUserEmail = '', pUserActive = '', pUserTypeId = '', pCompanyId = '', pPassword = '', IsNew = false;

//        if (_currentRow != '') {
//            pUserName = _currentRow.find("td:eq(1)").text();
//            pUserCode = _currentRow.find("td:eq(0)").text();
//            pUserActive = _currentRow.find("td:eq(3)").text();
//            pUserEmail = _currentRow.find("td:eq(2)").text();
//            pUserTypeId = _currentRow.find("td:eq(5)").text();
//            pPassword = _currentRow.find("td:eq(6)").text();
//            IsNew = false; $('#txtUserCode').attr('disabled', true);
//        }
//        else { IsNew = true; _userid = 0; $('#txtUserCode').removeAttr('disabled'); }
//        $("#txtUserCode").val(pUserCode);
//        $("#txtUserName").val(pUserName);
//        $("#txtUserEmail").val(pUserEmail);
//        $("#txtPassword,#txtConfPassword").val(pPassword);
//        var nActive = (pUserActive.toUpperCase() == 'YES') ? 1 : 0;
//        if (nActive == 1) $('#chkActive').attr('checked', 'checked');
//        //selUserActive
//        $('#selUserActive').val(nActive).trigger('change');
//        $('#selUserType').val(pUserTypeId).trigger('change');
//        _selectedUserId = _userid;
//        $('#modal_detail').modal('show');
//    } catch (e) {
//        alert(e);
//    }
//};

//$('#btnSavePopUp').on('click', function () {

//    var _tempId = (opMode == "INSERT") ? 0 : _selectedUserId;
//    SaveNewUser(_tempId);
//});

//var SaveNewUser = function (userId) {
//    var userTypeid = $("#selUserType").val();

//    var loginid = $('#txtUserCode').val();
//    var name = $('#txtUserName').val();
//    var mailid = $('#txtUserEmail').val();
//    var secCode = $('#txtPassword').val();
//    var cSecCode = $('#txtConfPassword').val();
//    var isActive = ($('#chkActive')[0].checked);
//    ValidateEmail(mailid);

//    if (Str(mailid).length == 0) toastr.error("Please enter User Email", "Supplier OnBoarding");//added by Kalpita on 18/01/2023
//    else if (Str(name).length == 0) toastr.error("Please enter User Name", "Supplier OnBoarding");
//    else if (Str(loginid).length == 0) toastr.error("Please enter User Code", "Supplier OnBoarding");
//    else if (Int(userTypeid) == 0) toastr.error("Please select User Type", "Supplier OnBoarding");
//    else if (Str(secCode).length == 0) toastr.error("Please enter valid password", "Supplier OnBoarding");
//    else if (Str(cSecCode).length == 0) toastr.error("Please enter valid confirmation password", "Supplier OnBoarding");
//    else if (Str(secCode) != Str(cSecCode)) toastr.error("Confirm password is mismatched", "Supplier OnBoarding");
//    else if (!IsValidUserEmail) { toastr.error("Email is Invalid", "Supplier OnBoarding"); }
//    else {
//        var isNewUser = (userId == 0) ? true : false;
//        var userdata = JSON.stringify([userTypeid, name, mailid, loginid, isActive, secCode, cSecCode, userId, cUserCode]);
//        CheckUserExists(JSON.stringify([loginid, userId, mailid]), userdata, loginid, isNewUser);
//    }
//};

//function CheckUserExists(jsondata, userdata, loginid, isNewUser) {
//    $.ajax({
//        type: "POST", async: false, url: pathname + "/UserAdministration/ValidateUserDetails", data: { "formData": jsondata },
//        success: function (response) {
//            try {
//                var isDuplicateUser = response.isExists[0]; var isDuplicateEmail = response.isExists[1];
//                var flag = (isNewUser) ? 'Inserted' : 'Updated';
//                //if (isNewUser)
//                // {
//                if (isDuplicateUser == false) {
//                    if (isDuplicateEmail == false) {
//                        SaveUserDetails(userdata);
//                    }
//                    else if (isDuplicateEmail == true) {
//                        toastr.error("Email already exists", "Supplier OnBoarding");
//                    }
//                }
//                else {
//                    toastr.error("User Code '" + loginid + "' already exists", "Supplier OnBoarding");
//                }

//                //}
//                //else {
//                //    SaveUserDetails(userdata, flag, loginid);
//                //}
//            }
//            catch (err) {
//                toastr.error("Unable to check if user exists.", "Supplier OnBoarding");
//            }
//        },
//        error: function (response) {
//            toastr.error("Unable to check if user exists.", "Supplier OnBoarding");
//        }
//    });
//};

//function SaveUserDetails(userdata) {
//    var target = document.querySelector("#Modal_Detail_Container");
//    var blockUI = new KTBlockUI(target, {
//        overlayClass: "bg-white bg-opacity-10"
//    });
//    blockUI.block();
//    $.ajax({
//        type: "POST", async: false, url: pathname + "/UserAdministration/SaveUser", data: { "formData": userdata },
//        success: function (response) {
//            try {
//                if (Int(response) > 0) {
//                    $('#modal_detail').modal('hide');
//                    Swal.fire({
//                        text: 'User Details updated successfully.',
//                        icon: "success",
//                        buttonsStyling: !1,
//                        confirmButtonText: "Ok",
//                        customClass: { confirmButton: "btn btn-primary" },
//                        showClass: { popup: 'animate__animated animate__fadeInDown' },
//                        hideClass: { popup: 'animate__animated animate__fadeOutUp' }
//                    }).then((function (e) {
//                        clearFilter();
//                        nEditing = -1; _userid = _selectedUserId = 0; LoadUserMaster();


//                    }));



//                }
//                else {
//                    toastr.error("Unable to Save. Exception - " + returnID, "Supplier OnBoarding"); _userid = 0;
//                }

//            }
//            catch (err) {
//                toastr.error("Unable to save data.", "Supplier OnBoarding"); _userid = 0;

//            }
//            blockUI.release();
//            blockUI.destroy();
//        },
//        error: function (response) {
//            toastr.error("Unable to save data.", "Supplier OnBoarding");
//            blockUI.release();
//            blockUI.destroy();
//        }
//    });
//};

//function DeleteUser(userid) {
//    var jsondata = JSON.stringify({ UserId: Int(userid) });
//    $.ajax({
//        type: "POST",
//        url: pathname + "/UserAdministration/DeleteUserDetails",
//        data: jsondata,
//        contentType: "application/json; charset=utf-8",
//        datatype: "json",
//        success: function (response) {
//            try {
//                if (Int(response.DelUser) > 0) {
//                    toastr.success("User Details deleted successfully.", "Supplier OnBoarding");
//                    nEditing = -1; LoadUserMaster();
//                }
//                else {
//                    toastr.error("Unable to Delete User.", "Supplier OnBoarding");
//                }
//            }
//            catch (err) {
//                toastr.error("Unable to Delete User", "Supplier OnBoarding");
//            }
//        },
//        error: function (response) {
//            toastr.error("Unable to Delete User", "Supplier OnBoarding");
//        }
//    });
//};

//UserType

function LoadUserTypeModuleAccess(Usertypeid) {

    try {
        var rdVal = Math.floor(Math.random() * 100) + 1;
        // $('#btnNew').hide();
        $("#tblUserTypeModuleAccess").DataTable().clear().draw().destroy();;
        oUsertypeTable = $("#tblUserTypeModuleAccess").DataTable({
            "fixedHeader": {
                header: true,
            },
            "scrollY": '40vh',
            "scrollCollapse": true,
            "scrollX": true,
            "processing": true, // for show progress bar
            "serverSide": false, // for process server side
            "filter": true, // this is for disable filter (search box)
            "orderMulti": false, // for disable multiple column at once
            "lengthMenu": [
                [50, 10, 25, 50, -1],
                ['10 rows', '25 rows', '50 rows', 'Show all']
            ],
            "pageLength": 10,
            "bDestroy": true,
            "ajax": {
                "url": pathname + "/UserAdministration/FilterUserTypeModuleAccessData",
                "data": { 'UserTypeId': Usertypeid },
                "type": "POST",
                "datatype": "json",
                //"dataSrc": function (json) {
                //    var a = json;
                //},
                "error": function (XMLHttpRequest, textStatus, errorThrown) {
                        
                }
            },
            "columns": [
               
                { "data": "module_Desc", "name": "Module Name", "autoWidth": true, "sClass": "ps-5" },
                { "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true },
                { "data": "moduleaccessid", "name": "ModuleAccessID", "autoWidth": true, "sClass": "hide_column" },
                { "data": "moduleid", "name": "ModuleID", "autoWidth": true, "sClass": "hide_column" },
                { "data": "accesS_LEVEL", "name": "Edit", "autoWidth": true, "sClass": "hide_column" }
            ],
            "initComplete": function (data) {
                $('#lblTotalRecord').text(data._iRecordsTotal);
                //var dataTable = document.getElementById("tblUserTypeModuleAccess");
                //var rowCount = dataTable.rows.length;
                var dataTable = $('#tblUserTypeModuleAccess').DataTable();
                var rowCount = dataTable.rows().count();


                if (rowCount > 0) {
                    $("#btneditusertype").show();
                    $("#btnadduser").hide();

                }
                else {
                    $("#btneditusertype").hide();
                    $("#btnadduser").show();
                }
            }
        });
        setTimeout(function () {
            $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
        }, 200);
        $('#kt_drawer_filter_close').click();
        $('.dataTable thead tr th').removeClass('text-end');
    }
    catch (e) {
        alert(e);
    }
};

function InitializeUserTypeModal(_doctype) {
    switch (_doctype) {
        case 'UserTypeDetail': ShowUserTypeDetail(); break;
        default: break;
    }
};

//function ShowUserTypeDetail() {
//    opMode = "INSERT";
//    $('#modal_Usertypedetail').modal('show');
//};

//$('#btnSaveUTypePopUp').on('click', function () {
//    SaveUserType();
//});

//function validateUserType() {
//    var errormsg = '';
//    if ((errormsg == "") && (Str($('#txtUserType').val()).trim() == null || Str($('#txtUserType').val()).trim() == ""))
//        errormsg = "Please enter User Type.";
//    if (errormsg == "") {
//        CerCode = Str($('#txtUserType').val()).trim();
//        var chkDup = CheckDuplicate_UserType(Str($('#txtUserType').val()).trim());
//        if (chkDup) errormsg = "User Type already exists.";
//    }
//    if ((errormsg == "") && ($('#selCopyFrom').val() == null || $('#selCopyFrom').val() == "")) {
//        errormsg = "Please Select User Type to copy from";
//    }
//    return errormsg;
//};

//function CheckDuplicate_UserType(value) {
//    var res = false;
//    $.ajax({
//        type: "POST",
//        async: false,
//        url: pathname + "/UserAdministration/ValidateUserTypeDetails",
//        data: "{'USERTYPEDESCR':'" + value + "'}",
//        contentType: "application/json;charset=utf-8",
//        dataType: "json",
//        success: function (response) {
//            res = response.IsExists;
//        },
//        failure: function (response) { toastr.error(response.IsUsrTypeExists, "Supplier OnBoarding"); },
//        error: function (response) { toastr.error("Error in Checking Duplicate User Type " + response.responseJSON.Message, "Supplier OnBoarding"); }
//    });
//    return res;
//};

//function SaveUserType() {
//    var _list = [];
//    var _errormsg = validateUserType();
//    if (_errormsg != "") toastr.error(_errormsg, "Supplier OnBoarding");
//    else {
//        var cUserTypeCode = Str($('#txtUserType').val());
//        _list.push("USERTYPEDESCR" + "|" + cUserTypeCode.toUpperCase());
//        _list.push("USERTYPEID_COPY" + "|" + Str($('#selCopyFrom').val()).split('|')[0]);
//        saveUsertypeDetails(_list);
//    }
//};

//function saveUsertypeDetails(fieldValues) {
//    var list = [];
//    for (var i = 0; i < fieldValues.length; i++) { list.push(fieldValues[i]); }
//    var data2send = JSON.stringify(list);
//    $.ajax({
//        type: "POST", async: false, url: pathname + "/UserAdministration/SaveUsertypeDetails", data: { "formData": data2send },

//        success: function (response) {
//            if (response.length > 0) {
//                Swal.fire({
//                    text: 'User Type saved Successfully.',
//                    icon: "success",
//                    buttonsStyling: !1,
//                    confirmButtonText: "Ok",
//                    customClass: { confirmButton: "btn btn-primary" },
//                    showClass: { popup: 'animate__animated animate__fadeInDown' },
//                    hideClass: { popup: 'animate__animated animate__fadeOutUp' }
//                }).then((function (e) {
//                    nEditing = -1;
//                    $('#modal_Usertypedetail').modal('hide');
//                    LoadUserTypeCombo(response);

//                }));
//            }
//            else toastr.error("Unable Save Usertype.", "Supplier OnBoarding");
//        },
//        failure: function (response) {
//            toastr.error("Failure in saving Details", "Supplier OnBoarding");
//        },
//        error: function (response) {
//            toastr.error("Error in saving Details", "Supplier OnBoarding");
//        }
//    });
//};

//UsertypeModuleaccess

//function editRow(nRow) {
//    var aData = $('#tblUserTypeModuleAccess').dataTable().fnGetData(nRow); var jqTds = $('>td', nRow);
//    var rowvalue = '<div class="ps-5">' + aData['access_Value_Text'] + '</div>';
//    var detTag = '<div style="text-align:center;"><a href="#" data-toggle="tooltip" data-doctype="Update"><i class="fa fa-save"></i></span>' +
//        ' <a href="#" data-toggle="tooltip" data-doctype="Cancel"><i class="fa fa-ban"></i></span></div>';
//    jqTds[1].innerHTML = '<select class="form-select form-select-sm" id="cboNewAccess" style="width:100%" onchange="getselectedrows(this)" class="fullWidth data-select"><option></option>' + Str(SelectAccessValue(Str(rowvalue))) + '</select>';
//    jqTds[4].innerHTML = "1";
//};

//function getselectedrows(e) {
//    var _list = [];
//    var ModuleAccessid = Str(oTable.fnGetData(e.parentNode.parentNode).moduleaccessId);
//    var ActionName = Str(oTable.fnGetData(e.parentNode.parentNode).module);
//    $(e.parentNode.parentNode).addClass("row" + e.parentNode.parentNode._DT_RowIndex);
//    $(e.parentNode).addClass("accesschanged");
//    rowvalue = jQuery(e).find('option:selected')[0].text;
//    rowid = jQuery(e).find('option:selected')[0].value;
//    oTable.fnGetData(e.parentNode.parentNode)[1] = '<div id="' + rowid + '" class="editable">' + rowvalue + '</div>';

//    _list.push("MODULEACCESSID" + "|" + ModuleAccessid);
//    _list.push("ACCESS_LEVEL" + "|" + Str(rowvalue));
//    _list.push("ACCESS_LEVEL_NUMBER" + "|" + rowid);
//    _selectedModAccess += ActionName + ' with Access Level:' + Str(rowvalue) + ', ';
//    UseraccessList.push(_list);

//};

//function restoreRow(nRow) {
//    var aData = $('#tblUserTypeModuleAccess').dataTable().fnGetData(nRow); var jqTds = $('>td', nRow);
//    for (var i = 0, iLen = jqTds.length; i < iLen; i++) {
//        if (i == 0) { } else {
//            _cellval = (jqTds[i].innerHTML.indexOf("select") == -1) ? jqTds[i].innerHTML : _cellval = aData[i]; $('#tblUserTypeModuleAccess').dataTable().fnUpdate(_cellval, nRow, i, false);
//        }
//    } $('#tblUserTypeModuleAccess').dataTable().fnDraw();
//};

//function gridCancelEdit(nNew, nEditing, oTable) {
//    var nRow = $('.DTTT_selected');
//    nRow = nEditing; restoreRow(nRow, oTable); $(nRow).removeClass("DTTT_selected selected");
//};

//function saveRow() {
//    if (UseraccessList.length > 0) {
//        var cAuditRemarks = 'ModuleAccess Details- ' + _selectedModAccess.replace(/,(?=[^,]*$)/, '') + ' updated for Usertype[' + _filterUserType + '] by ' + cUserCode;
//        saveUsertypeModuleAccess(UseraccessList);
//    }
//    else { toastr.error('Please select the access rights.', "Supplier OnBoarding"); }
//};

//var saveUsertypeModuleAccess = function (List) {
//    var result = -1; var data2send = { "formData": JSON.stringify(List) };

//    $.ajax({
//        type: "POST", async: false, url: pathname + "/UserAdministration/SaveUsertypeModuleAccess", data: { "formData": JSON.stringify(List) },
//        success: function (response) {
//            result = response.SaveUserAccess;
//            Swal.fire({
//                text: 'User Access Rights updated successfully.',
//                icon: "success",
//                buttonsStyling: !1,
//                confirmButtonText: "Ok",
//                customClass: { confirmButton: "btn btn-primary" },
//                showClass: { popup: 'animate__animated animate__fadeInDown' },
//                hideClass: { popup: 'animate__animated animate__fadeOutUp' }
//            }).then((function (e) {
//                nEditing = -1;
//                UseraccessList = [];
//                $('#btnSave,#btnCancel').hide();
//                $('#btnNew').show();
//                LoadUserTypeModuleAccess();
//            }));
//        },
//        failure: function (response) { toastr.error(response.d, "Supplier OnBoarding"); },
//        error: function (response) { toastr.error("Error in saving Module Access Data : " + response.responseJSON.Message, "Supplier OnBoarding"); }
//    });
//    return result;
//};

//$('.showmodal').on('click', function () {
//    var _div = $(this).parent();
//    var _doctype = $(_div).attr('data-doctype');
//    var _userid = $(_div).attr('data-userid');
//    var _currentRow = $(this).closest("tr");
//    opMode = "UPDATE";
//    ShowUserDetail(_userid, _currentRow);
//});

//function clearModal() { $('#txtUserType').val(""); $("#selCopyFrom").select2("val", ""); };

//$('#btnClearFilter').click(function () {
//    clearFilter();
//});
//function clearFilter() {
//    $("#txtFltrUserCode").val('');
//    $("#txtFltrUserName").val('');
//    $("#selFtrUserType").val('').trigger('onchange');

//    clear();
//}
//function clear() {
//    userName = ''; menuName = ''; reportType = ''; userCode = ''; userType = ''; opMode = ""; userTypeFilter = ""; _filterUserType = ''; _filterUserTypeId = '';
//    _filterTags = []; _selectedUserId = '';
//}

//$('#btnCancel').click(function () {
//    LoadUserTypeModuleAccess();
//});

//$('#btnNew').click(function () {
//    _selectedUserId = '';
//    if (selectedtab == 'Users') {
//        opMode = "INSERT";
//        ShowUserDetail(0, '');
//    }
//    else if (selectedtab == 'UserAccess Rights') {
//        InitializeUserTypeModal('UserTypeDetail');
//    }
//});

//function LoadUserTypeCombo(_data) {
//    var _options = '<option></option>';
//    $.each(_data, function (i, data) {
//        _options += '<option value="' + data.usertypeId + '">' + data.userTypeDescr + '</option>'
//    });
//    $('#selUserType').empty().html(_options); $('#selFtrUserType').empty().html(_options);
//}
function LoadUserTypeModuleEDIT(Usertypeid) {

    try {
        var rdVal = Math.floor(Math.random() * 100) + 1;

        
        // $('#btnNew').hide();
        $("#tblUserTypeModuleEDIT").DataTable().clear().draw().destroy();;
        oUsertypeEDITTable = $("#tblUserTypeModuleEDIT").DataTable({
            "language": {
                "infoFiltered": ""
            },
            "scrollY": '40vh',
            "scrollCollapse": true,
            "scrollX": true,
            "processing": true,
            "serverSide": false,
            "filter": true,
            "orderMulti": false,
            "lengthMenu": [
                [ 10, 25, 50, -1],
                ['10 rows', '25 rows', '50 rows', 'Show all']
            ],
            "pageLength": -1,
            "bDestroy": true,
            "ajax": {
                "url": pathname + "/UserAdministration/FilterUserTypeModuleEDIT",
                "data": { 'UserTypeId': Usertypeid },
                "type": "POST",
                "datatype": "json",
                
                "error": function (XMLHttpRequest, textStatus, errorThrown) {

                }
            },
            "columnDefs":
                [{
                    "targets": [0],
                    "visible": false,
                    "searchable": false
                },
                {
                    "targets": "_all",
                    "defaultContent": "-"
                }
                ],
            "columns": [
                { "data": "rowNumber", "name": "", "autoWidth": true },
                { "data": "module_Desc", "name": "Module Name", "autoWidth": true },
                /*{ "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true },*/
                {
                    "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true, "render": function (data, type, full, meta) {
                        var opts = '';
                        opts += '<option value="0" ' + (full.accesS_VALUE_TEXT == 'No Access' || full.accesS_VALUE_TEXT == null ? 'Selected' : '') + ' >' + 'No Access' + '</option>';
                        opts += '<option value="1" ' + (full.accesS_VALUE_TEXT == 'ReadOnly' ? 'Selected' : '') + ' >' + 'Readonly' + '</option>';
                        opts += '<option value="2" ' + (full.accesS_VALUE_TEXT == 'Add/Update' ? 'Selected' : '') + ' >' + 'Add/Update' + '</option>';
                        opts += '<option value="3" ' + (full.accesS_VALUE_TEXT == 'Full Access' ? 'Selected' : '') + ' >' + 'Full Access' + '</option>';
                        $('#lbluserTpye').text(full.usertypedescr);
                        return ' <select class="form-select fs-7 xyz" tabindex="-1" data-control="select2" id = "ddlaccess_' + full.moduleid + '" > ' + opts + '</select>';
                    }
                }
                //{ "data": "moduleaccessid", "name": "ModuleAccessID", "autoWidth": true, "sClass": "hide_column" },
                //{ "data": "moduleid", "name": "ModuleID", "autoWidth": true, "sClass": "hide_column" },
                //{ "data": "accesS_LEVEL", "name": "Edit", "autoWidth": true, "sClass": "hide_column" }
            ],
            "initComplete": function (data) {
                $('.xyz').select2();
                //var dataTable = $('#tblUserTypeModuleEDIT').DataTable();
                //var rowCount = dataTable.rows().count();


                //if (rowCount > 0) {
                //    $("#btneditusertype").show();
                //    $("#btnadduser").hide();

                //}
                //else {
                //    $("#btneditusertype").hide();
                //    $("#btnadduser").show();
                //}
            }
            
        });
        $('#aSave').click(function (e) {
            //SaveRows();
        });

        $('#aCancel').click(function (e) {
            CancelEdit();
        });
        isEdit = 0;
        //setTimeout(function () {
        //    /*$.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();*/
        //}, 200);
        //$('#kt_drawer_filter_close').click();
        //$('.dataTable thead tr th').removeClass('text-end');
    }
    catch (e) {
        alert(e);
    }
};

function LoadUserTypeModuleADD() {

    try {
        var rdVal = Math.floor(Math.random() * 100) + 1;


        // $('#btnNew').hide();
        $("#tblUserTypeModuleADD").DataTable().clear().draw().destroy();;
        oUsertypeAddable = $("#tblUserTypeModuleADD").DataTable({
            "language": {
                "infoFiltered": ""
            },
            "scrollY": '40vh',
            "scrollCollapse": true,
            "scrollX": true,
            "processing": true,
            "serverSide": false,
            "filter": true,
            "orderMulti": false,
            "lengthMenu": [
                [50, 10, 25, 50, -1],
                ['10 rows', '25 rows', '50 rows', 'Show all']
            ],
            "pageLength": 10,
            "bDestroy": true,
            "ajax": {
                "url": pathname + "/UserAdministration/ADDNewUserType",
                "data": { },
                "type": "POST",
                "datatype": "json",

                "error": function (XMLHttpRequest, textStatus, errorThrown) {

                }
            },
            "columnDefs":
                [{
                    "targets": [0],
                    "visible": false,
                    "searchable": false
                },
                {
                    "targets": "_all",
                    "defaultContent": "-"
                }
                ],
            "columns": [
                { "data": "moduleID", "name": "", "autoWidth": true },
                { "data": "module_Desc", "name": "Module Name", "autoWidth": true },
                /*{ "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true },*/
                {
                    "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true, "render": function (data, type, full, meta) {
                        var opts = '';
                        opts += '<option value="0" ' + (full.accesS_VALUE_TEXT == 'No Access' || full.accesS_VALUE_TEXT == null ? 'Selected' : '') + ' >' + 'No Access' + '</option>';
                        opts += '<option value="1" ' + (full.accesS_VALUE_TEXT == 'ReadOnly' ? 'Selected' : '') + ' >' + 'Readonly' + '</option>';
                        opts += '<option value="2" ' + (full.accesS_VALUE_TEXT == 'Add/Update' ? 'Selected' : '') + ' >' + 'Add/Update' + '</option>';
                        opts += '<option value="3" ' + (full.accesS_VALUE_TEXT == 'Full Access' ? 'Selected' : '') + ' >' + 'Full Access' + '</option>';
                       
                        return ' <select class="form-select fs-7 xyz" tabindex="-1" data-control="select2" id = "newddlaccess_' + full.moduleID + '" > ' + opts + '</select>';
                    }
                }
                //{ "data": "moduleaccessid", "name": "ModuleAccessID", "autoWidth": true, "sClass": "hide_column" },
                //{ "data": "moduleid", "name": "ModuleID", "autoWidth": true, "sClass": "hide_column" },
                //{ "data": "accesS_LEVEL", "name": "Edit", "autoWidth": true, "sClass": "hide_column" }
            ],
            "initComplete": function (data) {
                $('.xyz').select2();
                //var dataTable = $('#tblUserTypeModuleEDIT').DataTable();
                //var rowCount = dataTable.rows().count();


                //if (rowCount > 0) {
                //    $("#btneditusertype").show();
                //    $("#btnadduser").hide();

                //}
                //else {
                //    $("#btneditusertype").hide();
                //    $("#btnadduser").show();
                //}
            }

        });
        $('#aSave').click(function (e) {
            //SaveRows();
        });

        $('#aCancel').click(function (e) {
            CancelEdit();
        });
        isEdit = 0;
        //setTimeout(function () {
        //    /*$.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();*/
        //}, 200);
        //$('#kt_drawer_filter_close').click();
        //$('.dataTable thead tr th').removeClass('text-end');
    }
    catch (e) {
        alert(e);
    }
};


