var oTable = null;
var oUsertypeEDITTable = null;
$(document).ready(function () {
    $('#divTableTools').remove();
    $('#nav_userAdmin').addClass("show");
    $('#menuuserroles .menu-link').addClass("active");
    oUsertypeEDITTable = $('#tblUserTypeModuleAccess').dataTable();
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });

    $("#ddlUsertype").on("change", function () {
        var Usertypeid = $('#ddlUsertype option:selected').val();
        IntializeAccesstable(Usertypeid);

    });
    $('#btneditusertype').on("click", function () {
        var Usertypeid = $('#ddlUsertype option:selected').val();
        if (Usertypeid != '' && Usertypeid > 0) {
            LoadUserTypeModuleEDIT(Usertypeid)
            $('#modalItems').modal('show');
        }
        else {
            Swal.fire('Warning', 'Please select user type to proceed!', 'warning');
        
        }
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
        SaveUserAccessDetails(selectedOptions, Usertypeid)

    })
});
function SaveUserAccessDetails(SelectedOptions, usertypeid) {
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
                            $('#modalItems').modal('hide');
                            IntializeAccesstable(usertypeid);
                            
                            //window.open(pathname + "/UserAdministration/UsersAdmin", '_self');
                        }));

                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Warning', response.msg, 'warning').then((function (e) {
                                

                            }));
                        }
                        else {
                            Swal.fire('Error', 'Unable to save user access details', 'error').then((function (e) {
                                

                            }));
                        }

                    }
                }
                catch (err) {
                    Swal.fire('Error', 'Unable to save user access details', 'error').then((function (e) {
                        
                    }));
                }
            },
            error: function (response) {

                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                    Swal.fire('Unable to save user access details', "Please enter unique code", 'error');
                }
                else Swal.fire('Unable to save user access details', response.responseText, 'error');
            }
        });
    }
}
function IntializeAccesstable(Usertypeid) {
    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var count = 0;
    oTable = $("#tblUserTypeModuleAccess").DataTable({
        "language": {
            "infoFiltered": ""
        },
        //"scrollY": '50vh',
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
        "pageLength": 10, // Depending on your requirement, you may set it to a different value
        "bDestroy": true,
        "ajax": {
            "url": pathname + "/UserAdministration/FilterUserTypeModuleAccessData",
            "data": { 'UserTypeId': Usertypeid },
            "type": "POST",
            "datatype": "json",
            
            "error": function (XMLHttpRequest, textStatus, errorThrown) {

            }
        },
        "columnDefs": [
            /*{ "visible": isaccess, "targets": 7 }*/
        ],
        "columns": [
            { "data": "module_Desc", "name": "Module Name", "autoWidth": true, "sClass": "ps-5" },
            { "data": "accesS_VALUE_TEXT", "name": "Access Value", "autoWidth": true },
            { "data": "moduleaccessid", "name": "ModuleAccessID", "autoWidth": true, "sClass": "hide_column" },
            { "data": "moduleid", "name": "ModuleID", "autoWidth": true, "sClass": "hide_column" },
            { "data": "accesS_LEVEL", "name": "Edit", "autoWidth": true, "sClass": "hide_column" }

        ],
        "initComplete": function (data) {
            //$('#DOLinesTotalRecord').text(data._iRecordsTotal);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        }

    });
    
}
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
                [10, 25, 50, -1],
                ['10 rows', '25 rows', '50 rows', 'Show all']
            ],
            "pageLength": -1,
            "bDestroy": true,
            "ajax": {
                "url": pathname + "/UserAdministration/FilterUserTypeModuleAccessDataEdit",
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
                
            }

        });
        $('#aSave').click(function (e) {
            //SaveRows();
        });

       
        isEdit = 0;
        
    }
    catch (e) {
        
    }
};
