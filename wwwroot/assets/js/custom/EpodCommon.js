var pathname = '', cUserCompany = '';
var slModuleAction = [];
var nSave = 0;
var AccessLevels = {
    NoAccess: 0,
    Readonly: 1,
    Write: 2,
    FullAccess: 3
};


jQuery(document).ready(function () {
    pathname = window.location.pathname;
    var temp = UserDefaultData;
    if (window.location.hostname.indexOf('localhost') > -1) pathname = '';
    if (pathname.indexOf('/') > -1) { pathname = '/' + pathname.split('/')[1]; }
    SetLogo();
    SetSessionValues();
    SetupActionButtons();
    RemoveEmptyMenu();
});

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

function SetLogo() {
    //$('#slcompany').text(UserDefaultData.epodUser.les_company_details.company_Description);
    if (UserDefaultData.epodUser.les_company_details.base64minLogo) {

        var iconmime = getMimeType(UserDefaultData.epodUser.les_company_details.base64minLogo);
        var logomime = getMimeType(UserDefaultData.epodUser.les_company_details.base64Logo);
        var imageUrl = "data:" + logomime + ";base64," + UserDefaultData.epodUser.les_company_details.base64Logo;
        var shortlogo = "data:" + iconmime + ";base64," + UserDefaultData.epodUser.les_company_details.base64minLogo;
        $('#companylogo').attr('src', imageUrl);
        $('#companylogo').attr('alt', UserDefaultData.epodUser.les_company_details.company_Description);
        $('#shorticon').attr('href', shortlogo);
        $('#shorticon').attr('type', iconmime);

    }
    else {
        $('#companylogo').attr('alt', UserDefaultData.epodUser.les_company_details.company_Description);
    }
}

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


function SetSessionValues() {
    if (UserDefaultData != null) {
        var companyid = UserDefaultData.companyid; // Get the company ID
        var data = UserDefaultData.epodUser.list_Application_Module_Access.find(function (module) {
            return module.companyId === companyid;
        });
        
        if (data) {
            slModuleAction[data.moduleId] = data.access_Level;
        }
    }
    else {
        Swal.fire('Error','Something went wrong at our side, Please contact support for more assistance!','error')
    }
   
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
var SearchData = (oTable) => {
    const filterSearch = document.querySelector('[data-kt-filter="search"]');
    filterSearch.addEventListener('keyup', function (e) {
        oTable.search(e.target.value).draw();
    });
}