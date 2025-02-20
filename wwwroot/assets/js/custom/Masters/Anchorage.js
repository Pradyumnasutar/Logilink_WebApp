var tbl = null;
var anchorID = 0;
$(document).ready(function () { Initialize(); _detailpageLoaded = true; });

function Initialize() {
    $('#nav_masters').addClass("show");
    $('#menuAnchorage .menu-link').addClass("active");
    $('#btnexportanchorage').click(function () {
        PrintAnchorage();
    });
    var isaccess = false;
    if (slModuleAction[5] > AccessLevels.Readonly) {
        isaccess = true;
    }
    if (slModuleAction[5] < AccessLevels.FullAccess) {
        $('.Delete').hide();
    }
    oTable = $("#tblData").DataTable({
        "language": {
            "infoFiltered": ""
        },
        "scrollY": '50vh',
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
        "columnDefs":
            [{
                "targets": [0],
                "visible": false,
                "searchable": false,
                "defaultContent": "0"
            },
            {
                "targets": [3],
                "visible": isaccess

            }],
        "pageLength": 10,
        "bDestroy": true
    });

    SearchData(oTable);
};

function AddNew(e) {
    var AllID = document.querySelectorAll(`[id^="divEdit"]`);
    for (var i = 0; i < AllID.length; i++) {
        var EachID = AllID[i];
        document.getElementById(EachID.id).style.pointerEvents = "none";
        var editLink = document.getElementById(EachID.id).querySelector('.btn-color-primary');
        if (editLink) {
            editLink.style.color = 'gray';
            editLink.classList.remove('btn-color-primary');
            editLink.classList.add('btn-color-gray-500');
            editLink.setAttribute('onclick', 'return false;');
        }
    }
    $('#btnAddnew').css('background-color', 'gray');
    $('#btnAddnew').prop('disabled', true);
    this.pkId = 0; 
   
    oTable = oTable.row.add(['0',
        '<input type="text" class="form-control fs-7" id="txtCode" maxlength="5" placeHolder="Anchorage Code" />',
        '<input type="text" class="form-control fs-7" id="txtDescr" maxlength="250" placeHolder="Description"/>',
        '<div id="divSave"><a class="btn btn-sm btn-sm btn-link btn-color-success btn-active-color-success me-5 mb-2" href="#" onclick="SaveRow()">Save</a><a class="btn  btn-sm btn-link btn-color-gray-500 btn-active-color-gray-500 me-5 mb-2" href="#" onclick="CancelEdit()">Cancel</a></div>']).draw();
    $(e).addClass('Show');
}

function EditRow(e) {
    var AllID = document.querySelectorAll(`[id^="divEdit"]`);
    for (var i = 0; i < AllID.length; i++) {
        var EachID = AllID[i];
        var editLink = document.getElementById(EachID.id).querySelector('.btn-color-primary');
        if (editLink) {
            editLink.style.color = 'gray';
            editLink.classList.remove('btn-color-primary');
            editLink.classList.add('btn-color-gray-500');
            editLink.setAttribute('onclick', 'return false;');
        }
        $('#btnAddnew').css('background-color', 'gray');
        $('#btnAddnew').prop('disabled', true);
    }
    
    var selectedTr = $(e).parents('tr')[0];
    if (selectedTr != undefined) {
        editRow(selectedTr);
    }
}
function editRow(id) {
    $('#btnAddnew').css('background-color', 'gray');
    $('#btnAddnew').prop('disabled', true);
    var aData = $('#tblData').dataTable().fnGetData(id); var jqTds = $('>td', id);
    var detTag = '<div id="divSave"><a class="btn btn-sm btn-link btn-color-success btn-active-color-success me-5 mb-2" href="#" onclick="SaveRow()">Save</a><a class="btn btn-sm  btn-link btn-color-gray-500 btn-active-color-gray-500 me-5 mb-2" href="#" onclick="CancelEdit()">Cancel</a></div>';
    this.anchorID = Str(aData[0]);
    jqTds[0].innerHTML = '<input type="text" class="form-control fs-7" id="txtCode" maxlength="5" placeHolder="Code"  value="' + Str(aData[1]) + '"/>';
    jqTds[1].innerHTML = '<input type="text" class="form-control fs-7" id="txtDescr" maxlength="250" placeHolder="Description" value="' + Str(aData[2]) + '"/>';
    jqTds[2].innerHTML = detTag;
}

function SaveRow() {
    var code = $('#txtCode').val();
    var descr = $('#txtDescr').val();
    if (code != '' && descr != '') {
        $.ajax({
            type: "POST",async: false,
            url: pathname + "/Masters/SaveAnchorage",
            data: { "id": anchorID, "desc": descr,"code": code },
            success: function (response) {
                try {
                    if (response.result == true) {
                        var msg = 'Anchorage added successfully!';
                        if (anchorID > 0) msg = 'Anchorage updated successfully!';

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
                            location.reload();
                        }));
                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Error', response.msg, 'error');
                        }
                        else {
                            Swal.fire('Error', 'Unable to save Anchorage', 'error');
                        }
                    }
                }
                catch (err) {
                    Swal.fire('Error', 'Unable to save Anchorage', 'error');
                }
                //this.UOMID = 0;
            },
            error: function (response) {
               // UOMID = 0;
                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                    Swal.fire('Unable to save Anchorage', "Please enter unique code", 'error');
                }
                else Swal.fire('Unable to save Anchorage', response.responseText, 'error');
            }
        });
    }
    else Swal.fire('Validate', 'Please enter all the fields', 'warning');

}

function CancelEdit() {
    if (anchorID > 0) {
        var result = Swal.fire({
            title: '', text: "Are you sure you want to Cancel the change ?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No'
        }).then((result) => {
            if (result.isConfirmed) {
                
                location.reload();               
            }
        });
    }
    else location.reload();
}

function PrintAnchorage() {
    var target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }
    $.ajax({
        type: 'POST',
        url: pathname + "/Masters/ExportAnchorage",
        data: {

            _anchorID: anchorID
        },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            var filename = "";
            var disposition = xhr.getResponseHeader('Content-Disposition');
            if (disposition && disposition.indexOf('attachment') !== -1) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches !== null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }

            var blob = new Blob([data], { type: 'application/pdf' }, filename);
            var url = window.URL.createObjectURL(blob);

            // For download
            var linkElement = document.createElement('a');
            linkElement.href = url;
            linkElement.download = filename;  // Sets the filename for download
            document.body.appendChild(linkElement);
            linkElement.click();
            document.body.removeChild(linkElement);
            window.URL.revokeObjectURL(url); // Clean up the object URL
            blockUI.release();
            blockUI.destroy();
            // For preview
            //window.open(url, '_blank'); // Opens the PDF in a new tab for preview
        },
        error: function (xhr, status, error) {
            blockUI.release();
            blockUI.destroy();
            console.log("Error: " + error);
        }
    });
}