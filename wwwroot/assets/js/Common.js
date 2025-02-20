
var applNo = '', crewName = ''; var Params = {};
var slModuleAction = []; var oTableAttach = null; var tableAttach = $(document.getElementById('dataGridAttach'));
var cHost = '', cUserName = "", cUserid = 0, adminid = 0, cUserAccessrights = null, cUserTypelvl = 0,
    cAddrType = '', cUserCode = '', cAddressId = 0, cUserType = 0, cUserAddressid = 0;
var cSubAddressId = 0, cSubAddrName = '', cSubAddrType = '', cBuyerCompany = '', cUserEmail = '', pathname = '', cUserCompany = '';
var slBuyerIds = [], slBuyerNames = []; var _selectedBuyerIds = '', _selectedBuyerNames = '';//added by kalpita on 10/01/2023
var nSave = 0;
var _detailpageLoaded = false;
var Outboundstatus = [];
var Deliveyrdstatus = [];
var AllModules = [];
var AccessLevels = {
    NoAccess: 0,
    Readonly: 1,
    Write: 2,
    FullAccess: 3
};


jQuery(document).ready(function () {
    $('.copyright').css('display', 'block');
    //$('#kt_aside_footer').show();
    $('#ddlSelCompany').select2({
        allowClear: false,
        width: '100%'
    });
    const selectElement = $('#ddlSelCompany');
    if (UserDefaultData != null && UserDefaultData != undefined)
        var temp = UserDefaultData.companiesaccess;
    selectElement.on('select2:unselecting', function (e) {
        e.preventDefault(); // Prevent deselecting
    });
    selectElement.on('change', function (e) {
        e.stopPropagation();

        var Id = $('#ddlSelCompany').val();
        var code = $(this).find('option:selected').text();
        if (Id > 0) {

            Swal.fire({
                title: '', text: "Are you sure you want to switch company to '" + code + "' ?",
                showCancelButton: true, confirmButtonText: 'Yes',
                cancelButtonText: 'No',
            }).then((result) => {
                if (result.isConfirmed) {

                    MoveToNewCompany(Id);
                }
                else {
                    location.reload();

                }
            });
        }
    });
    ApplyToggle();
    pathname = window.location.pathname;
    if (window.location.hostname.indexOf('localhost') > -1) pathname = '';
    if (pathname.indexOf('/') > -1) { pathname = '/' + pathname.split('/')[1]; }
    const selectcomp = document.getElementById('ddlSelCompany');
    if (hasOptions(selectcomp)) {
        SetSelCompany();
    }

    InitializeCommon();
    SetSessionValues();
    SetupActionButtons();
    RemoveEmptyMenu();
    SetLogo();//saved in local stoage 

});
function redirectToProject() {
    window.open(pathname + "/Home/RedirectToEPOD", "_blank")

}
function SetSelCompany() {
    // var OptionData = Ajax(pathname + "/Home/GetCompanyList", {}, true);
    var options = '<option></option>'
    if (UserDefaultData != undefined && UserDefaultData.company_detail_data != null && UserDefaultData.company_detail_data.length > 0) {
        for (var i = 0; i < UserDefaultData.company_detail_data.length; i++) {
            options += '<option value=' + UserDefaultData.company_detail_data[i].companyId + '>' + UserDefaultData.company_detail_data[i].company_Description + '</option>';

        }

    }
    $('#ddlSelCompany')[0].innerHTML = options;
    $('#ddlSelCompany').val(UserDefaultData.companyid);
}
function selectOptionByValue(selectElement, value) {
    selectElement.val(value).change(); // Sets the value and triggers the change event
}
function MoveToNewCompany(companyid) {
    if (companyid > 0) {
        var resData = Ajax(pathname + "/Home/SwitchCompany", { "companyId": companyid }, true);
        if (resData.result) {
            window.location = pathname + "/Home/Index";
        }
        else {
            Swal.fire('', resData.msg, 'error');
        }
    }
    else {
        Swal.fire('', 'Oops, something went wrong, please contact support', 'error');
    }
}

function SetLogo() {

    var Id = $('#ddlSelCompany').find('option:selected').text();
    //var Companydata = localStorage.getItem('comp' + Id);
    //  var Companydata = Enumerable.from(UserDefaultData.company_detail_data).where(x => x.compnayId == UserDefaultData.companyid).toArray(); 
    var Companydata = UserDefaultData.company_detail_data.filter(obj => {
        return obj.companyId === UserDefaultData.companyid
    })
    if (Companydata != '' && Companydata != null && Companydata != undefined && Companydata.length > 0) {
        var parsed = Companydata[0];
        if (parsed.base64Logo) {

            var iconmime = getMimeType(parsed.base64minLogo);
            var logomime = getMimeType(parsed.base64Logo);
            var imageUrl = "data:" + logomime + ";base64," + parsed.base64Logo;
            var shortlogo = "data:" + iconmime + ";base64," + parsed.base64minLogo;
            $('#companylogo').attr('src', imageUrl);
            $('#companylogo').attr('alt', parsed.company_Description);
            $('#shorticon').attr('href', shortlogo);
            $('#shorticon').attr('type', iconmime);

        }
        else {
            $('#companylogo').attr('alt', parsed.company_Description);
        }
    }
    else {
        var resData = Ajax(pathname + "/Authenticate/GetLogo", {}, true);
        if (resData.result) {
            //    localStorage.setItem('comp' + Id, JSON.stringify(resData));
            var iconmime = getMimeType(resData.minlogo);
            var logomime = getMimeType(resData.logo);
            var imageUrl = "data:" + logomime + ";base64," + resData.logo;
            var shortlogo = "data:" + iconmime + ";base64," + resData.minlogo;
            $('#companylogo').attr('src', imageUrl);
            $('#companylogo').attr('alt', resData.company);
            $('#shorticon').attr('href', shortlogo);
            $('#shorticon').attr('type', iconmime);

        }
        else {
            $('#companylogo').attr('alt', resData.company);
        }
    }

}


//function SetLogo() {

//    var Id = $('#ddlSelCompany').find('option:selected').text();
//    //var Companydata = localStorage.getItem('comp' + Id);
//    var Companydata = UserDefaultData.companiesaccess[UserDefaultData.companyid];
//    if (Companydata != ''&&Companydata!=null) {
//        var parsed = JSON.parse(Companydata);
//        if (parsed.result) {

//            var iconmime = getMimeType(parsed.minlogo);
//            var logomime = getMimeType(parsed.logo);
//            var imageUrl = "data:" + logomime + ";base64," + parsed.logo;
//            var shortlogo = "data:" + iconmime + ";base64," + parsed.minlogo;
//            $('#companylogo').attr('src', imageUrl);
//            $('#companylogo').attr('alt', parsed.company);
//            $('#shorticon').attr('href', shortlogo);
//            $('#shorticon').attr('type', iconmime);

//        }
//        else {
//            $('#companylogo').attr('alt', parsed.company);
//        }
//    }
//    else {
//        var resData = Ajax(pathname + "/Authenticate/GetLogo", {}, true);
//        if (resData.result) {
//        //    localStorage.setItem('comp' + Id, JSON.stringify(resData));
//            var iconmime = getMimeType(resData.minlogo);
//            var logomime = getMimeType(resData.logo);
//            var imageUrl = "data:" + logomime + ";base64," + resData.logo;
//            var shortlogo = "data:" + iconmime + ";base64," + resData.minlogo;
//            $('#companylogo').attr('src', imageUrl);
//            $('#companylogo').attr('alt', resData.company);
//            $('#shorticon').attr('href', shortlogo);
//            $('#shorticon').attr('type', iconmime);

//        }
//        else {
//            $('#companylogo').attr('alt', resData.company);
//        }
//    }

//}

function getMimeType(base64) {
    // Common base64 prefixes
    var prefixMap = {
        "PHN2Z": "image/svg+xml", // SVG tag start
        "PD94b": "image/svg+xml", // XML declaration for SVG
        "77U": "image/svg+xml",   // SVG start when base64url encoded
        "/9j/": "image/jpeg",
        "iVBORw0KGgo": "image/png",
        "R0lGODdh": "image/gif",
        "PD94bWwgdmVyc2lvbj0iMS4wIiA/Pz4=": "image/svg+xml", // XML declaration for SVG
        "PHN2Zy": "image/svg+xml" // SVG tag
    };

    for (var prefix in prefixMap) {
        if (base64.startsWith(prefix)) {
            return prefixMap[prefix];
        }
    }

    // Default to PNG if no match is found
    return "image/png";
}
function hasOptions(selectElement) {
    return selectElement.options.length > 0;
}
function SetupActionButtons() {
    var nAccessLevel = 0; if (slModuleAction.length == 0 || slModuleAction == null) { slModuleAction = JSON.parse(cUserAccessrights); }
    var ActionButtons = $(".ModuleAccess"); var ActionBtn = null; var cAccessLevel = 0, cBtnAccLevel = 0; //simmy11032019
    if (slModuleAction != null) {
        for (var i = 0; i < ActionButtons.length; i++) {
            ActionBtn = $(ActionButtons[i]);
            cBtnAccLevel = ActionBtn[0].getAttribute("data-access_level");
            if (ActionBtn[0].getAttribute("data-module") != null) { nAccessLevel = slModuleAction[ActionBtn[0].getAttribute("data-module")]; }
            if (cBtnAccLevel != null) {
                if (cBtnAccLevel.indexOf(nAccessLevel) > -1) ActionBtn.show();
                else ActionBtn.remove();
            }
        }

    }
    else {//added on 15.02.2023 to remove all actions if access rights not available for usertype
        for (var i = 0; i < ActionButtons.length; i++) {
            ActionBtn = $(ActionButtons[i]);
            ActionBtn.remove();
        }
    }
};
function RemoveEmptyMenu() { //Raviprasad 18-08-2023 removes Sub-menu and menu if they don't have Moduleaccess and if empty
    var ActionButtons = $(".menu-accordion"); var ActionBtn = null; var Submenu1 = null;
    if (ActionButtons != null) {
        for (var i = 0; i < ActionButtons.length; i++) {
            ActionBtn = $(ActionButtons[i]);
            var Classes = ActionBtn[0].attributes.class.value;
            if (!Classes.includes("ModuleAccess")) {
                var XX = ActionBtn[0].querySelectorAll(".menu-sub-accordion");
                if (XX[0].childElementCount == 0) {
                    ActionBtn.remove();

                }
                else {
                    var Submenu = XX[0];
                    for (var j = 0; j < XX[0].childElementCount; j++) {
                        Submenu1 = $(Submenu.children[j]);
                        if (!Submenu1[0].attributes.class.value.includes("ModuleAccess")) {
                            Submenu1.remove();
                            j--;
                        }
                        else ActionBtn.show();
                    }
                    if (Submenu.childElementCount == 0) {
                        ActionBtn.remove();

                    }
                }

            }
        }
    }
}

function Str(v) { if (v == undefined || v == null || v == 'undefined' || v.toString() == '[object Object]' || v.toString() == 'NaN') return ''; else return v.toString().trim(); };
function Int(v) { if (Str(v) == '' || isNaN(parseInt(v))) return 0; else return parseInt(v); };
function Float(v) { if (Str(v) == '' || isNaN(parseFloat(v))) return 0; else return parseFloat(v); };
function Len(obj) { if (window.DOMParser != undefined) return Object.keys(obj).length; else return Object.size(obj); };
function GetDictionary(XML) {
    var hTable = {};
    var _xml = loadXML(XML); if (_xml != "" && _xml.childNodes.length > 0) { var _sCount = 3; for (var i = _sCount; i < _xml.childNodes[0].childNodes.length; i = i + 2) { var el = _xml.childNodes[0].childNodes[i]; var _childTable = {}; for (var k = 1; k < el.childNodes.length; k = k + 2) { _childTable[el.childNodes[k].tagName] = el.childNodes[k].textContent; } var count = Object.keys(hTable).length; hTable[count] = _childTable; } return hTable; } else return {};
};
function loadXML(txt) { if (txt != -1 && txt != null && txt != undefined) { if (window.DOMParser != undefined) { parser = new DOMParser(); xmlDoc = parser.parseFromString(txt, "text/xml"); } else /* code for IE */ { xmlDoc = new ActiveXObject("Microsoft.XMLDOM"); xmlDoc.async = false; xmlDoc.loadXML(txt); } return xmlDoc; } else return ''; };


$("#idLoginLink").on("click", function () {
    $('#idLoginLink').css('text-decoration', 'underline');

});
function ChangePassword() {
    $('#current_password').val('');
    $('#new_password').val('');
    $('#confirm_password').val('');
    $('#modalChangePwd').modal('show');
}
function UpdatePassword() {
    var pwd = { "current": null, "new_pwd": null, "confirm": null }
    // Check invalid chars //
    pwd.current = $('#current_password').val();
    pwd.new_pwd = $('#new_password').val();
    pwd.confirm = $('#confirm_password').val();
    if (pwd.current.trim().length == 0) {
        Swal.fire('', 'Please enter Current Password', 'warning');
    }
    else if (pwd.new_pwd.trim().length == 0) {
        Swal.fire('', 'Please enter New Password', 'warning');
    }
    else if (pwd.confirm.trim().length == 0) {
        Swal.fire('', 'Please enter Confirm Password', 'warning');
    }
    else if (pwd.new_pwd.trim().length < 8 || pwd.confirm.trim().length < 8) {
        Swal.fire('', 'Password must contain minimum 8 chars ', 'warning');
    }
    else {
        $.ajax({
            type: "POST", async: false, url: pathname + "/Home/UpdatePassword", data: { "formData": JSON.stringify(pwd) },
            success: function (response) {
                try {
                    if (response.success == true) {
                        Swal.fire('Updated', 'Your password has been updated successfully.', 'success');
                    }
                    else {
                        Swal.fire('Failed', response.message, 'error');
                    }
                }
                catch (err) {
                    Swal.fire('Failed', 'Unable to update password, Please contact to LeS Support team.<br/> Exception : ' + err, 'error');
                }
            },
            error: function (response) {
                Swal.fire('Failed', 'Unable to update password<br/> Exception : ' + response.responseText, 'error');
            }
        });
    }
}

function SetSessionValues() {
    if (UserDefaultData != null) {
        slModuleAction = UserDefaultData.companiesaccess[UserDefaultData.companyid];
    }
    else {
        $.ajax({

            "url": pathname + "/Authenticate/GetSessionValues", async: false,
            //"data":'',
            "datatype": "json",
            success: function (response) {
                slModuleAction = response;
            },
            failure: function (response) {
                //alert(response.responseText);
            },
            error: function (response) {
                //alert(response.responseText);
            },
        });
    }
}
function LogOut() {
    var msg = "You Have Successfully Signed Out!";
    //location.href = pathname + "/Authenticate/Logout?msg=" + msg;
    NavigatePage("/Authenticate/Logout?msg=" + msg);
}

function NavigatePage(page) {
    if (nSave == 1) {
        Swal.fire({
            text: 'There are unsaved changes in the current page.Do you want to continue?',
            icon: "warning",
            showDenyButton: true,
            showCancelButton: false,
            confirmButtonText: 'Yes',
            denyButtonText: 'No',
            allowOutsideClick: false,
            modal: true,
            showClass: { popup: 'animate__animated animate__fadeInDown' },
            hideClass: { popup: 'animate__animated animate__fadeOutUp' }
        }).then((function (e) {
            if (e.isConfirmed) { window.location.href = pathname + page; }

        }));
    }
    else { window.location.href = pathname + page; }

}

function NavigateMaintenancePage(Title, Module) {
    var data = JSON.stringify({ 'Title': Title, 'Module': Module });
    location.href = "/Maintenance/Authenticate?Title=" + Title + "&Module=" + Module;
}

function GetSortingFormattedDate(dateyear, datemonth, dateday) { var str = 'yyyy/mm/dd'; str = str.replace("yyyy", dateyear); str = str.replace("mm", datemonth); str = str.replace("dd", dateday); return str; };

function ShowDetail(e) {
    try {
        document.body.style.cursor = 'wait';
        var EncriptText = $(e).attr("data-id");
        var href = pathname + "/Transaction/ModalDetails/id=" + EncriptText;
        $('#Modal_Detail_Container').empty();
        $('#Modal_Detail_Container').load(href, function (e) {
            $('#modal_detail').modal('show');
            document.body.style.cursor = 'auto';
        });
    }
    catch (e) {
        document.body.style.cursor = 'auto';
    }


}

function DisposeBlockUI(blockUI) {
    blockUI.release();
    blockUI.destroy();
}

// Updated by Sayak //
function setToggle() {
    var clsVal = document.getElementById('kt_aside_toggle').className;

    if (clsVal == 'btn btn-icon w-auto px-0 btn-active-color-primary aside-toggle') {
        sessionStorage.setItem('toggle', 'true');
    } else {
        sessionStorage.setItem('toggle', 'false');
    }

    setTimeout(function () {
        $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
    }, 200);
}


// Updated by Sayak //
function ApplyToggle() {
    var toggleAside = sessionStorage.getItem('toggle');
    if (toggleAside == 'true') {
        var ktBody = document.getElementById('kt_body');
        if (ktBody != null) {
            var attr = ktBody.getAttribute("data-kt-aside-minimize")
            if (attr == 'on') {
                // console.log("already Toggle aside");
            }
            else {
                ktBody.setAttribute("data-kt-aside-minimize", "on");
            }
        }
        var ktToggleAside = document.getElementById('kt_aside_toggle');
        if (ktToggleAside != null) {
            ktToggleAside.setAttribute("class", "btn btn-icon w-auto px-0 btn-active-color-primary aside-toggle active");
        }
        //  $('#kt_aside').show();
    }
    else {
        var ktBody = document.getElementById('kt_body');
        if (ktBody != null) {
            var attr = ktBody.getAttribute("data-kt-aside-minimize")
            if (attr == 'on') {
                ktBody.removeAttribute("data-kt-aside-minimize");
            }
            else {
                //   console.log("already Toggle");
            }
        }
        var ktToggleAside = document.getElementById('kt_aside_toggle');
        if (ktToggleAside != null) {
            ktToggleAside.setAttribute("class", "btn btn-icon w-auto px-0 btn-active-color-primary aside-toggle");
        }
        //   $('#kt_aside').show();
    }
    $('#expandedFooter').show();
    $('#minimizedFooter').hide();


    $('#kt_aside').show(); $('#kt_wrapper').show();
    //toggleFooter()
}

/*document.getElementById('kt_aside_toggle').addEventListener('click', function () {
    var expandedFooter = document.getElementById('expandedFooter');
    var minimizedFooter = document.getElementById('minimizedFooter');

    if (expandedFooter.style.display === 'none') {
        expandedFooter.style.display = 'block';
        minimizedFooter.style.display = 'none';
    } else {
        expandedFooter.style.display = 'none';
        minimizedFooter.style.display = 'block';
    }
});*/
/*function toggleFooter() {
    if ($('#kt_aside_toggle').hasClass('active')) {
        $('#expandedFooter').show();
        $('#minimizedFooter').hide();
    } else { 
        $('#expandedFooter').hide();
        $('#minimizedFooter').show();
    }
}
$('#kt_aside_toggle').on('click', function () {
    $('#kt_aside').toggleClass('active'); 
    toggleFooter(); 
});
*/



function LoadVersionInfo() {
    var version = Ajax(pathname + "/Authenticate/Get_Version", '', false);
    if (version != 1) {
        $('#lblVersion')[0].innerText = version;
    }

}




function Ajax(u, p, t) {
    try {
        var ret = null;
        $.ajax({
            type: 'POST',
            //contentType: "application/json; charset=utf-8",
            //datatype: "json",
            async: false, url: u, data: p,
            success: function (ds) {
                ret = ds;
            },
            error: function (e) {
                if (t == false) { ret = -1 } else { if (e.responseJSON != undefined) ret = e.responseJSON.Message; else ret = e.responseText; }
            }
        }); return ret;
    } catch (e) {
        //alert(e);
    }
}

var SearchData = (oTable) => {
    const filterSearch = document.querySelector('[data-kt-filter="search"]');
    filterSearch.addEventListener('keyup', function (e) {
        oTable.search(e.target.value).draw();
    });
}

function InitializeCommon() {

    $('.tagify').tagsinput({
        allowDuplicates: true,
        itemValue: 'id',  // this will be used to set id of tag
        itemText: 'label' // this will be used to set text of tag
    });
    $('.form-select').select2({
        allowClear: true
    });
    $('#btnClear_Filter').on("click", function () {

        $('select[id*="sel"]').each(function () {
            $(this).val('').trigger('change');
        });
        $('#kt_accordion_filter_body .form-control').val('');

        //$('.dateField').flatpickr({
        //    enableTime: false,
        //    minuteIncrement: 1,
        //    inline: false,
           
           
        //});
    });
    $('.modal').modal({ backdrop: 'static', keyboard: false })
}

function convertToDate(dateString) {
    //  Convert a "dd/MM/yyyy" string into a Date object
    var d = dateString.split("/");
    //var dat = new Date(d[2]  + '/' + d[1] + '/' + d[0]);
    var dat = new Date(d[2], (d[1] - 1), d[0]);
    return dat;
}
function convertDateToStrDate(date) {
    if (date != null) {
        var formattedDate = new Date(date);
        var formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
            ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
            formattedDate.getFullYear() + ' ' +
            ('0' + formattedDate.getHours()).slice(-2) + ':' +
            ('0' + formattedDate.getMinutes()).slice(-2);
        return formattedDateString;
    }
    else {
        return '';
    }
}

function convertToDateTime(dateString) {
    //  Convert a "dd/MM/yyyy" string into a Date object
    var d = dateString.split("/");
    //var dat = new Date(d[2]  + '/' + d[1] + '/' + d[0]);
    var y = d[2].split(" ");
    var t = y[1].split(":");
    var h = t[0];
    var m = t[1];
    var dat = new Date(y[0], (d[1] - 1), d[0], h, m);
    return dat;
}
function ExportExcel() {
    var target = document.querySelector("#tblData");
    var blockUI = new KTBlockUI(target, {
        overlayClass: "bg-white bg-opacity-10"
    });
    blockUI.block();
    try {


        var moduleName = Print_Key;
        $.ajax({
            type: "POST", async: true, url: pathname + "/Masters/ExportExcel", data: { 'moduleName': moduleName },
            success: function (response) {
                try {
                    if (response[0] == 'OK') {
                        //window.location = pathname + response[1];
                        window.open(pathname + response[1], "_blank");
                    }
                    else {
                        Swal.fire('No record found.');
                        blockUI.release();
                        blockUI.destroy();
                    }
                }
                catch (err) {
                    Swal.fire('Error', err, "error");
                    blockUI.release();
                    blockUI.destroy();
                }
                blockUI.release();
                blockUI.destroy();
            },
            error: function (response) {
                Swal.fire('Error', response.responseText, "error");
                blockUI.release();
                blockUI.destroy();
            }
        });
    }
    catch (err) {
        blockUI.release();
        blockUI.destroy();
    }

    return false;
}


function ExportPDF() {

    var target = document.querySelector("#tblData");
    var blockUI = new KTBlockUI(target, {
        overlayClass: "bg-white bg-opacity-10"
    });
    blockUI.block();
    try {
        var moduleName = Print_Key;
        $.ajax({
            type: "POST", async: true, url: pathname + "/Masters/ExportPDF",
            data: { 'moduleName': moduleName },
            success: function (response) {
                try {
                    if (response[0] == 'OK') {
                        window.open(pathname + response[1], "_blank");
                    }
                    else {
                        Swal.fire('No record found.');
                        blockUI.release();
                        blockUI.destroy();
                    }
                }
                catch (err) {
                    Swal.fire('Error', err, "error");
                    blockUI.release();
                    blockUI.destroy();
                }
                blockUI.release();
                blockUI.destroy();
            },
            error: function (response) {
                Swal.fire('Error', response.responseText, "error");
                blockUI.release();
                blockUI.destroy();
            }
        });
    }
    catch (err) {
        blockUI.release();
        blockUI.destroy();
    }

    return false;
}

$("div .Detail input").on('input', function (e) {
    if (_detailpageLoaded) {
        nSave = 1;
    }
});

$('div .Detail select').on('change', function (ev) {
    if (_detailpageLoaded) {
        nSave = 1;
    }
});

$('div .Detail textarea').on('change', function (e) {
    if (_detailpageLoaded) {
        nSave = 1;
    }
});

$("table tbody").on('input', 'input ', function () {
    if (_detailpageLoaded && $(this).attr('type') !== 'radio' && $(this).attr('type') !== 'checkbox') {
        nSave = 1;
    }
});


$("table tbody").on('input', 'select ', function () {
    if (_detailpageLoaded) {
        nSave = 1;
    }
});
function ClearValues() { nSave = 0; _detailpageLoaded = false; }

function GetDateOnly(datetime) {
    if (datetime != null) {
        let date = new Date(datetime);
        date.setHours(0);
        date.setMinutes(0);
        date.setSeconds(0);
        date.setMilliseconds(0);
        return date.toISOString();
    }
    else return '';
}

document.addEventListener("DOMContentLoaded", function () {
    moveCompanySelect();
    window.addEventListener('resize', moveCompanySelect);
});

function moveCompanySelect() {
    const screenWidth = window.innerWidth;
    const originalSelect = document.getElementById('original-company-select');
    const responsivePlaceholder = document.getElementById('responsive-company-select');
    const originalContainer = document.getElementById('original-container');

    if (screenWidth <= 1000) {
        if (!responsivePlaceholder.contains(originalSelect)) {
            responsivePlaceholder.appendChild(originalSelect);
            responsivePlaceholder.classList.add('custom-style');

        }
        originalSelect.style.display = 'flex';
    } else {
        if (!originalContainer.contains(originalSelect)) {
            originalContainer.appendChild(originalSelect);
            responsivePlaceholder.classList.remove('custom-style');

        }
        originalSelect.style.display = 'flex';
    }
}