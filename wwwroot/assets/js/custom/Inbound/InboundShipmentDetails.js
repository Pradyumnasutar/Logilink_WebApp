var result;
var oTable = null;
var oDOTable = null;
var Rtable = null;
var InboundShipmentId = null;
var Douploadtable = null;
var shipdoctable;
var poid = null;
var doid = 0;
var doLength = 0;
var postatusid = null;
$(document).ready(function () {
    InboundShipmentId = inboundid;
    $('#nav_InboundShipments').addClass("show");
    initSettabs();
    LoadInboundShipmentDetails();
    LoadInboundShipmentbyEsupplier()
    LoadPurchaseOrderDetails();

    LoadPurchaseOrderDetailsEsupplier();
    GetLinkedDOcountAndPOlineCount(poid)
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"

    });
    $('#btnuploadDO').click(function (e) {

        UploadDocDO();

    });
    $('#radiofulldelivery').change(function (e) {

        checkallanddisable();
        restoreTableState();
    });
    $('#radiosplitdelivery').change(function (e) {

        Uncheckallanddisable();
        makeQtyColumnEditable();
        validateQtyInput();
    });

    //MergePO();

    $('#tbOrderinfo').click(function (e) {
        $('#tbOrderinfo').css("background-color", "#40444d");
        $('#tbShippingDetails').css("background-color", "#787373");
        $('#tbOrderLines').css("background-color", "#787373");
        $('#tbDeliveryOrder').css("background-color", "#787373");

        $('#kt_tab_pane_1').addClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
        $('#kt_tab_pane_4').removeClass("show active");
    });
    $('#tbShippingDetails').click(function (e) {
        $('#tbShippingDetails').css("background-color", "#40444d");
        $('#tbOrderinfo').css("background-color", "#787373");
        $('#tbOrderLines').css("background-color", "#787373");
        $('#tbDeliveryOrder').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').addClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
        $('#kt_tab_pane_4').removeClass("show active");
    });
    $('#AssignDoModal').on('hidden.bs.modal', function () {
        localStorage.removeItem("DO_DOCUMENTS");
    });
    $('#btnAssignDo').click(function (e) {

        $('#txtdeliveryno').val('');
        $('#txtvesselname').val('');
        $('#txtjobno').val('');
        $('#dtdeliverydt').val('');
        Initializedoupload([]);
        $('#fileInputdo').val('');
        $('#AssignDoModal').modal('show');
        localStorage.removeItem("DO_DOCUMENTS");
        var pono = $('#txtpono').val();
        var id = $('#txtpono').attr('data-id');
        if (id > 0) {
            $('#showPOno').text(pono);
            InitialiseRemainedqtyTable(pathname + "/Inbound/GetVRemainingInternalOrderLines", id);
            LoadAttachments(pathname + "/Inbound/GetInboundShipmentDocuments", id);
            $('#radiofulldelivery').prop('checked', true);
            checkallanddisable();
        }


    })

    //$('#tbDeliveryOrder').click(function (e) {
    //    var pono = $('#txtpono').val();
    //    doid = $('#txtpono').attr('data-id');
    //    if (doid > 0) {
    //        InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", doid);
    //    }
    //})

    $('#tbOrderLines').click(function (e) {
        $('#tbOrderLines').css("background-color", "#40444d");
        $('#tbOrderinfo').css("background-color", "#787373");
        $('#tbShippingDetails').css("background-color", "#787373");
        $('#tbDeliveryOrder').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_4').removeClass("show active");
        $('#kt_tab_pane_3').addClass("show active");
    });

    $('#tbDeliveryOrder').click(function (e) {
        $('#tbDeliveryOrder').css("background-color", "#40444d");
        $('#tbOrderLines').css("background-color", "#787373");
        $('#tbOrderinfo').css("background-color", "#787373");
        $('#tbShippingDetails').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
        $('#kt_tab_pane_4').addClass("show active");
    });

    $(document).on('click', '.shipment-docc', function (e) {
        var _id = e.target.attributes['data-id'].value;
        DownloadOutboundAttachment(_id);
    })
    $(document).on('click', '.do-details', function (e) {
        var _id = e.target.attributes['data-id'].value;
        ViewDetails(_id);
    })
    $('#btnPrintDeliveryOrder').click(function (e) {
        console.log("Inside the print del event");
    });

    $(document).on('click', '#btnAssignConfirm', async function () {
        try {
            var table = $('#tblpartlines').DataTable();

            // Get total row count
            var totalRowCount = table.data().count();
            if (totalRowCount > 0) {
                var data = getCheckedRowsData();

                var poNumber = $('#txtpono').val();

                ValidateDODetails();
             
                if (data.length === 0) {
                    return Swal.fire({
                        icon: 'warning',
                        title: 'Warning',
                        text: 'Please check the lines to be attached for PO!'
                    });
                }

                const result = await Swal.fire({
                    title: '',
                    text: `Are you sure you want to attach ${data.length} lines to PO: '${poNumber}'?`,
                    showCancelButton: true,
                    confirmButtonText: 'Yes',
                    cancelButtonText: 'No'
                });

                if (result.isConfirmed) {
                    try {
                        const responseMessage = await AssignDOtoPO(data);
                        await Swal.fire({
                            text: responseMessage,
                            icon: "success",
                            buttonsStyling: false,
                            confirmButtonText: "Ok",
                            customClass: { confirmButton: "btn btn-primary" },
                            showClass: {
                                popup: 'animate__animated animate__fadeInDown'
                            },
                            hideClass: {
                                popup: 'animate__animated animate__fadeOutUp'
                            }
                        });
                        location.reload();

                    } catch (errorMessage)
                    {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: errorMessage
                        });
                    }
                }
            }
            else
            {
                Swal.fire({
                    icon: 'warning',
                    title: 'Warning',
                    text: 'No purchase order lines found!'
                });
            }
            
        } catch (error) {
            Douploadtable.clear().draw();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: error || 'An unexpected error occurred.'
            });
        }
    });
    $('#fileInputdo').on('change', function () {
        var formData = new FormData();
        var files = this.files;
        $.each(files, function (i, file) {
            formData.append('files', file); // Append each file
        });

        Initializedoupload(files);

        if (files != null && files != '' && files.length < 5) {
            $.ajax({
                url: pathname + '/Inbound/UploadDeliveryOrderDocuments',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.result) {
                        if (response.data.length > 0) {
                            localStorage.removeItem("DO_DOCUMENTS");
                            localStorage.setItem("DO_DOCUMENTS", response.data);
                        }
                        else {
                            localStorage.removeItem("DO_DOCUMENTS");
                        }
                    }
                    else {
                        localStorage.removeItem("DO_DOCUMENTS");
                        swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.msg
                        });
                    }

                },
                error: function () {
                    swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Something went wrong while uploading files!'
                    });
                }
            });
        }
        else {

            Initializedoupload([]);

            if (files.length > 5) {
                $('#fileInputdo').val('');
                swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Please uoload 5 files per delivery order!'
                });
            }
            else {
                $('#fileInputdo').val('');
                swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Something went wrong while uploading files!'
                });
            }

        }


    });

    
});
$(document).on('click', '#btnRelease', async function () {
    var pono = $('#txtpono').val(); // Get the PO number

    var internalOrderId = $('#txtpono').attr('data-id');
    var inboundShipId = $('#txtpono').attr('data-inboundid');

    try {
        // Display confirmation dialog
        const result = await Swal.fire({
            title: '',
            text: `Are you sure you want to release Inbound Shipment: '${pono}'?`, // Corrected variable name
            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'No'
        });

        if (result.isConfirmed) {
            try {
                // Release the inbound shipment
                await ReleaseInboundShipment(inboundShipId, internalOrderId); // Assuming this is an async function
            } catch (errorMessage) {
                console.error(errorMessage); // Log the error for debugging
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: errorMessage || 'An unexpected error occurred.' // Display error message
                });
            }
        }
    } catch (error) {

        console.error(error); // Log the error for debugging
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error || 'An unexpected error occurred.' // Display error message
        });
    }
});

async function GetLinkedDOcountAndPOlineCount(internalOrderId) {
    $.ajax({
        url: pathname + '/Inbound/GetLinkedDOandPOlinesCount',
        type: 'POST',
        data: { internalorderid: internalOrderId },
        success: function (response) {
            if (response.result && response.docount != -1 && response.polinecount != -1 && response.postatusid>-1) {
                var docount = response.docount;
                var polinescount = response.polinecount;
                
                //if (docount > 0 && polinescount == 0 && response.postatusid == 1) {
                //    $('#btnRelease').show(); 
                //}
                //else {
                //    $('#btnRelease').hide(); 
                //}
                //if (polinescount > 0) {
                //    $('#btnAssignDo').show();
                //}
            }
            
            
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            //$('#uploadMessage').text('Unable to upload file.');
        }
    });
}
async function ReleaseInboundShipment(inboundShipId,internalOrderId) {
    var dfghj = 0;
    $.ajax({
        url: pathname + '/Inbound/ReleaseInboundShipment',
        type: 'POST',
        data: { inboundShipmentId: inboundShipId, InternalOrderId: internalOrderId },
        success: function (response) {
            //$('#uploadMessage').text(response.message);
            var dfghj = 0;
            if (response.result) {
                var fghj = 0;
                Swal.fire({
                    text: response.msg,
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
                    //InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", poid, postatusid);
                }));
            }
            else {
                Swal.fire('Failed', 'Message : ' + response.msg, 'error');
            }


        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            //$('#uploadMessage').text('Unable to upload file.');
        }
    });
}

function LoadinboundDetails(_id) {
    $.ajax({
        url: pathname + '/Inbound/EncryptId',
        type: 'GET',
        data: { id: _id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/Inbound/InboundShipmentDetails?inboundshipmentid=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}
function ViewDetails(_id) {
    //console.log(_id)
    $.ajax({
        url: pathname + '/Orders/EncryptId',
        type: 'GET',
        data: { id: _id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/Orders/DeliveryOrderDetails?id=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}
function getFileTitlesFromTable() {
    var fileTitles = [];

    // Iterate over each row in the table body
    $(`#uploadedFilesTable tbody tr`).each(function () {
        // Get the title attribute from the first column
        var fileTitle = $(this).find('td:first span').attr('title');
        if (fileTitle) {
            fileTitles.push(fileTitle); // Add to the array if not empty
        }
    });

    return fileTitles; // Return the array of file titles
}
function getMatchingFileNamesWithoutExtension(abc, xyz) {
    // Helper function to remove file extension
    function removeExtension(fileName) {
        return fileName.split('.').slice(0, -1).join('.');
    }

    // Remove extensions from both arrays
    const abcWithoutExtensions = abc.map(file => removeExtension(file));

    // Find matching files in xyz by checking if baseName exists in each element of xyz
    const matchingFiles = xyz.filter(file => {
        const baseName = removeExtension(file);
        return abcWithoutExtensions.some(baseFile => baseName.includes(baseFile)); // Check if baseFile is part of the file in xyz
    });

    return matchingFiles; // Return the array of matched filenames from xyz
}

function Initializedoupload(files) {
    Douploadtable = $("#uploadedFilesTable").DataTable({
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
        "columnDefs": [{
            "targets": [0],
            "visible": true,
            "searchable": false,
            "defaultContent": "0"
        }],
        "pageLength": 10,
        "bDestroy": true
    });

    // Function to add files to the table
    function addRows(files) {
        // Check if files array is empty and remove all rows if true
        Douploadtable.clear().draw();


        // Loop through the files and add them as rows
        files.forEach(function (file) {
            var fileName = file.name;
            var displayName = fileName;

            // Trim file name to 20 characters with tooltip for full name
            if (fileName.length > 20) {
                displayName = fileName.slice(0, 8) + '...' + fileName.slice(-8);  // Trim to 20 characters (first 8 + ... + last 8)
            }

            // Add row to the DataTable
            Douploadtable.row.add([
                `<span title="${fileName}">${displayName}</span>`, // Display name with tooltip for full name
                `<button class="btn btn-sm btn-danger removeFile">Remove</button>` // Action to remove file
            ]).draw();
        });
    }


    // Call addRows with the files parameter (array of file objects)
    addRows(files);

    // Remove functionality for "Remove" button
    $("#uploadedFilesTable").on("click", ".removeFile", function () {
        var row = $(this).closest('tr');
        Douploadtable.row(row).remove().draw(); // Remove the row from DataTable
    });

    // Call SearchData function (if needed)
    // SearchData(Douploadtable);
}
function getDeliveryOption() {
    // Get the radio buttons
    const fullDelivery = document.getElementById("radiofulldelivery");
    const splitDelivery = document.getElementById("radiosplitdelivery");

    // Check which one is selected and return the corresponding value
    if (fullDelivery && fullDelivery.checked) {
        return 0;
    } else if (splitDelivery && splitDelivery.checked) {
        return 1;
    } else {
        // Optional: Handle the case where neither is selected
        console.error("No delivery option selected!");
        return null;
    }
}
function AssignDOtoPO(lines) {
    return new Promise((resolve, reject) => {
        var deliveryOrderNo = $('#txtdeliveryno').val();
        var vesselName = $('#txtvesselname').val();
        var jobNo = $('#txtjobno').val();
        var orderDate = $('#dtdeliverydt').val();
        var isFullOrSplit = getDeliveryOption();
        var internalOrderId = $('#txtpono').attr('data-id');
        var inboundShipId = $('#txtpono').attr('data-inboundid');
        var actualdocuments = getFileTitlesFromTable();
        var documents = getDODocuments();
        var actualdocs = getMatchingFileNamesWithoutExtension(actualdocuments, documents);
        $.ajax({
            url: `${pathname}/Inbound/AssignPOlinesToDo`,
            type: "GET",
            data: {
                strmodal: JSON.stringify(lines),
                docmodal: JSON.stringify(actualdocs),
                InternalOrderid: internalOrderId,
                Inboundshipmentid: inboundShipId,
                JobNo: jobNo,
                OrderNo: deliveryOrderNo,
                vesselname: vesselName,
                isfullorsplit: isFullOrSplit,
                OrderDt: orderDate
            },
            success: function (response) {
                if (response.result) {
                    resolve(response.msg);
                } else {
                    reject(response.msg);
                }
            },
            error: function () {
                reject("Failed to process the request. Please try again!");
            }
        });
    });
}
function getDODocuments() {
    const data = localStorage.getItem("DO_DOCUMENTS");
    if (data) {
        try {
            const documents = JSON.parse(data);
            return documents;
        } catch (error) {
            console.error("Error parsing DO_DOCUMENTS:", error);
            return [];
        }
    } else {
        return [];
    }
}
function ValidateDODetails() {
    try {
        var deliveryOrderNo = $('#txtdeliveryno').val();
        var VesselName = $('#txtvesselname').val();
        var Jobno = $('#txtjobno').val();
        var OrderDate = $('#dtdeliverydt').val();
        var isfullorsplit = getDeliveryOption();
        var proceed = true;
        const documents = getDODocuments();
        if (deliveryOrderNo != undefined && deliveryOrderNo.length > 0) { } else {
            throw "Please fill delivery order no!";

        }
        if (OrderDate != undefined && OrderDate != null && OrderDate != "") { } else {
            throw "Please fill delivery order date!";

        }
        if (isfullorsplit == 1 || isfullorsplit == 0) {

        }
        else {
            throw "Please check whether it is full delivery or split delivery.";
        }
        if (documents == null || documents == undefined || documents.length == 0) {
            throw "Please upload delivery order documents!";
        }
        if (documents != null && documents.length > 5) {
            $('#uploadedFilesTable').DataTable().clear().draw();
            throw "You can upload 5 files per delivery order!";
        }
    }
    catch (err) {

        throw err;

    }



}
function validateQtyInput() {
    // Add input event listener on the .editable-qty inputs
    $('#tblpartlines').on('input', '.editable-qty', function () {
        var maxQuantity = parseFloat($(this).data('max')); // Get the max quantity from data-max attribute
        var enteredValue = parseFloat($(this).val());

        // Ensure the value is within the valid range (0 to maxQuantity)
        if (enteredValue > maxQuantity) {
            $(this).val(maxQuantity); // Set the value to maxQuantity if it exceeds
            swal.fire({
                icon: 'warning',
                title: 'Warning',
                text: 'Quantity should not be greater than ' + maxQuantity
            });
        } else if (enteredValue < 0) {
            $(this).val(maxQuantity); // Set the value to 0 if it's less than 0
            swal.fire({
                icon: 'warning',
                title: 'Warning',
                text: 'Quantity can not be less than 0'
            });

        }
    });
}
function restoreTableState() {
    // Loop through each row and restore the original "Qty" column text
    $('#tblpartlines tbody tr').each(function () {
        var row = $(this);

        // Find the "Qty" column (4th column, index 3)
        var qtyCell = row.find('td').eq(3);
        var inputField = qtyCell.find('input');

        // Retrieve the original value from the data-original attribute
        var originalValue = inputField.data('original');

        // Restore the original content of the "Qty" column as text
        qtyCell.text(originalValue); // Set the value to the original value stored in data-original
    });

    // Optionally, remove the change event for validation as we no longer need it
    $('#tblpartlines').off('change', '.editable-qty');
    $('#tblpartlines').off('input', '.editable-qty');
}

function ClearModalFields() {
    $('#modallocation input').val('');
    $('#modallocation textarea').val('');
    $('#modallocation select').val('').trigger('change');
}

function checkallanddisable() {
    $('#selectAllCheckbox').prop('checked', true);
    //$('.row-select').prop('checked', true).prop('disabled', true);
    Rtable.rows().nodes().each(function (row, index) {
        $(row).find('.row-select').prop('checked', true).prop('disabled', true);
    });
    $('#selectAllCheckbox').prop('disabled', true);
}
function Uncheckallanddisable() {
    $('#selectAllCheckbox').prop('checked', true);
    Rtable.rows().nodes().each(function (row, index) {
        $(row).find('.row-select').prop('checked', true).prop('disabled', false);
    });
    $('#selectAllCheckbox').prop('disabled', false);
}


function initSettabs() {
    $('#tbOrderinfo').css("background-color", "#40444d");
    $('#tbShippingDetails').css("background-color", "#787373");
    $('#tbOrderLines').css("background-color", "#787373");

    $('#kt_tab_pane_1').addClass("show active");
    $('#kt_tab_pane_2').removeClass("show active");
    $('#kt_tab_pane_3').removeClass("show active");
}
function EncryptId(intid) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: pathname + '/Inbound/EncryptId',
            type: 'GET',
            data: { id: intid },
            success: function (response) {
                if (response.success) {
                    resolve(response.encryptedId);
                } else {
                    console.error("Error while encrypting ID: ", response.message);
                    reject(response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error while encrypting ID: ", error);
                reject(error);
            }
        });
    });
}


//#region Load Data
function LoadInboundShipmentDetails() {
    if (inboundid > 0) {
        EncryptId(inboundid)
            .then(encid => {
                return $.ajax({
                    url: pathname + '/Inbound/GetInboundShipmentDetails',
                    type: 'POST',
                    data: { inboundshipmentid: encid },
                    success: function (response) {
                        console.log("Inbound shipment details loaded successfully.");
                        FillShipmentInfo(response);
                        var d = 0;
                    },
                    error: function (xhr, status, error) {
                        console.error("Error while fetching shipment details: ", error);
                    }
                });
            })
            .catch(error => {
                console.error("Error in encryption or subsequent process: ", error);
            });
    }
}
function LoadInboundShipmentbyEsupplier() {
    if (quotationid > 0) {
        EncryptId(quotationid)
            .then(encid => {
                return $.ajax({
                    url: pathname + '/Inbound/GetInboundShipmentDetailsEsupplier',
                    type: 'POST',
                    data: { quotationid: encid },
                    success: function (response) {
                        console.log("Inbound shipment details loaded successfully.");
                        FillShipmentInfo(response);
                        var d = 0;
                    },
                    error: function (xhr, status, error) {
                        console.error("Error while fetching shipment details: ", error);
                    }
                });
            })
            .catch(error => {
                console.error("Error in encryption or subsequent process: ", error);
            });
    }
}
function LoadPurchaseOrderDetails() {
    return new Promise((resolve, reject) => {
        if (inboundid > 0) {
            EncryptId(inboundid)
                .then(encid => {
                    return $.ajax({
                        url: pathname + '/Inbound/GetPurchaseOrderDetails',
                        type: 'POST',
                        data: { inboundshipmentid: encid },
                        success: function (response) {
                            console.log("Purchase order details loaded successfully.");
                            poid = response.internalOrderId;
                            FillPurchaseOrderDetails(response);
                            //GetLinkedDOcountAndPOlineCount(poid)
                            LoadInternalOrderLines(poid);
                            postatusid = response.statusId;
                            InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", poid, postatusid);

                            //InitialiseRemainedqtyTable(pathname + "/Inbound/GetVRemainingInternalOrderLines",poid);


                        },
                        error: function (xhr, status, error) {
                            console.error("Error while fetching PO details: ", error);
                        }
                    });
                })
                .catch(error => {
                    console.error("Error in encryption or subsequent process: ", error);
                });
        }
    });
}
function LoadPurchaseOrderDetailsEsupplier() {
    return new Promise((resolve, reject) => {
        if (quotationid > 0) {
            EncryptId(quotationid)
                .then(encid => {
                    return $.ajax({
                        url: pathname + '/Inbound/GetPurchaseOrderDetailsEsupplier',
                        type: 'POST',
                        data: { quotationid: encid },
                        success: function (response) {
                            console.log("Purchase order details loaded successfully.");
                            poid = response.internalOrderId;
                            FillPurchaseOrderDetails(response);
                            LoadInternalorderLinesEsupplier(quotationid);
                            //InitialiseRemainedqtyTable(pathname + "/Inbound/GetVRemainingInternalOrderLines", poid);
                            postatusid = response.statusId;
                            InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", poid, postatusid)



                        },
                        error: function (xhr, status, error) {
                            console.error("Error while fetching PO details: ", error);
                        }
                    });
                })
                .catch(error => {
                    console.error("Error in encryption or subsequent process: ", error);
                });
        }
    });
}
function LoadInternalOrderLines(Poid) {
    if (Poid > 0) {
        EncryptId(Poid)
            .then(encid => {
                return InitialiseTable(pathname + '/Inbound/GetInternalOrderLines', encid)

                //$.ajax({
                //    url: pathname + '/Inbound/GetInternalOrderLines',
                //    type: 'POST',
                //    data: { InternalOrderid: encid },
                //    success: function (response) {
                //        populateDataTable(response);
                //        console.log("Purchase order lines loaded successfully.");

                //    },
                //    error: function (xhr, status, error) {
                //        console.error("Error while fetching PO lines: ", error);
                //    }
                //});
            })
            .catch(error => {
                console.error("Error in encryption or subsequent process: ", error);
            });
    }
}
function LoadInternalorderLinesEsupplier(quotationid) {
    if (quotationid > 0) {
        EncryptId(quotationid)
            .then(encid => {
                return InitialiseTable(pathname + '/Inbound/GetInternalOrderLinesEsupplier', encid)

                //$.ajax({
                //    url: pathname + '/Inbound/GetInternalOrderLines',
                //    type: 'POST',
                //    data: { InternalOrderid: encid },
                //    success: function (response) {
                //        populateDataTable(response);
                //        console.log("Purchase order lines loaded successfully.");

                //    },
                //    error: function (xhr, status, error) {
                //        console.error("Error while fetching PO lines: ", error);
                //    }
                //});
            })
            .catch(error => {
                console.error("Error in encryption or subsequent process esupplier: ", error);
            });
    }
}

//#endregion Load Data

//#region Fill Data
function FillShipmentInfo(Obj) {
    $("#txtinboundshipno").val(Obj.shipmentno);
    $("#txtstatus").val(Obj.status_desc);

    $("#txtjobOrderNo").val(Obj.jobOrderNo);
    if (UserDefaultData.isbuyer) {
        $("#txtparty").val(Obj.companyCode + '-' + Obj.companyName);
    }
    else {
        $("#txtparty").val(Obj.cust_code + '-' + Obj.cust_name);
    }

    $('#txtvessel').val(Obj.vessel_name);
    $('#txtdept').val(Obj.department);
    $('#txtconsignee').val(Obj.consignee);
    $('#txtforwarder').val(Obj.forwarder);
    $('#dttransaction').val(convertDateToStrDate(Obj.transactionDate));
    $('#dtetd').val(convertDateToStrDate(Obj.eta));
    $('#dteta').val(convertDateToStrDate(Obj.etd));
}
function FillPurchaseOrderDetails(Obj) {
    return new Promise((resolve, reject) => {
        $('#txtorderdesc').val(Obj.internal_Order_Desc);
        $('#txtpono').val(Obj.internal_Order_No);
        if (Obj.internalOrderId > 0) {
            $('#txtpono').attr('data-id', Obj.internalOrderId);
            $('#txtpono').attr('data-inboundid', Obj.inboundshipmentid);

        }

        $('#txtfrom').val(Obj.fromServicePoint);
        $('#txtto').val(Obj.toServicePoint);
        $('#txtcontNo').val(Obj.containerNumber);
        $('#txtContType').val(Obj.containerType);
        $('#txtContSize').val(Obj.containerSize);
        $('#txtSealno').val(Obj.sealNo);
        $('#txtBondedNo').val(Obj.bondedLotNo);
        $('#txtinwardpermit').val(Obj.inwardPermitNo);
        $('#dtorderdate').val(convertDateToStrDate(Obj.orderDate));
        $('#dtdeliverydate').val(convertDateToStrDate(Obj.delivery_Date));
        $('#txtfreightamt').val(Obj.frieghtAmount);
        $('#txtothercost').val(Obj.otherCost);
        $('#txtitemtotal').val(Obj.itemTotal);
        $('#txtPoamt').val(Obj.poAmount);
        $('#txtCurrency').val(Obj.currency);
    });

}

function InitialiseRemainedqtyTable(_url, encid) {
    var isaccess = false;
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var count = 0;
    Rtable = $("#tblpartlines").DataTable({
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
        "order": [[2, "asc"]],
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10, // Depending on your requirement, you may set it to a different value
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { internalorderid: encid },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var x = 0;
            },

            "dataSrc": function (data) {
                if (data.length > 0) {
                    $('#btnAssignDo').show();
                }
                else {
                    $('#btnAssignDo').hide();
                    $('#btnAssignConfirm').hide();
                }
               
                return data;
            }
        },
        "columnDefs": [
            { "targets": 0, "visible": false },
            {
                "targets": 4, // Targeting the "Qty" column (index 3)
                "width": "80px" // Set the width for the "Qty" column
            }
        ],
        "columns": [
            { "data": "internalLineId", "name": "internallineid", "autoWidth": true },
            {
                data: "", data: "Receipt Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<input type="checkbox" class="row-select select-checkbox" data-id="' + full.internalLineId + '">';
                }
            },
            { "data": "lineNo", "name": "Line No.", "autoWidth": true },
            { "data": "partName", "name": "Description", "autoWidth": true },
            { "data": "orderedQuantity", "name": "Qty", "autoWidth": true },
            { "data": "deliveredQuantity", "name": "Utilized  Qty", "autoWidth": true },
            { "data": "uom", "name": "UOM", "autoWidth": true },
            { "data": "remainingQuantity", "name": "Remaining Qty", "autoWidth": true },

            {
                data: "", data: "Receipt Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.receiptDate != null && full.receiptDate != '') {
                        return convertDateToStrDate(full.receiptDate);
                    }
                    else {
                        return '';
                    }

                }
            },
            {
                data: "", data: "Arrival Date", "autoWidth": true, "render": function (data, type, full, meta) {

                    if (full.arrivalDate != null && full.arrivalDate != '') {
                        return convertDateToStrDate(full.arrivalDate);
                    }
                    else {

                        return '';
                    }

                }
            },


        ],
        "initComplete": function (data) {
            $('#partTotalRecord').text(data.aoData.length);
            // Update "Select All" checkbox based on all rows
            var allChecked = $('.row-select:checked').length === Rtable.rows().data().length;
            $('#selectAllCheckbox').prop('checked', allChecked);
            checkallanddisable();
        },
        "drawCallback": function () {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);


        }

    });

    // Handle "Select All" checkbox
    $('#selectAllCheckbox').on('change', function () {
        var isChecked = $(this).is(':checked');
        Rtable.rows().nodes().each(function (row, index) {
            $(row).find('.row-select').prop('checked', isChecked);
        });
    });

    // Handle individual row checkbox selection (update "Select All" state)
    $('#tblpartlines').on('change', '.row-select', function () {
        var allChecked = $('.row-select:checked').length === Rtable.rows().data().length;
        $('#selectAllCheckbox').prop('checked', allChecked);
    });
}
function InitialiseTable(_url, encid) {
    var isaccess = false;
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var count = 0;
    oTable = $("#tblOrderLines").DataTable({
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
        "order": [[0, "asc"]],
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10, // Depending on your requirement, you may set it to a different value
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { InternalOrderid: encid },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var x = 0;
            },

            "dataSrc": function (data) {
                return data;

            }
        },
        "columnDefs": [
            { orderable: false, targets: 0 }
            /*{ "visible": isaccess, "targets": 7 }*/
        ],
        "columns": [
            { "data": "lineNo", "name": "Line No.", "autoWidth": true },
            { "data": "salesPartId", "name": "Item No.", "autoWidth": true },
            { "data": "partName", "name": "Description", "autoWidth": true },
            { "data": "qty", "name": "Qty", "autoWidth": true },
            { "data": "uom", "name": "UOM", "autoWidth": true },
            { "data": "qty_Rec", "name": "Rec Qty", "autoWidth": true },
            { "data": "part_Price", "name": "Unit Price", "autoWidth": true },
            { "data": "salesPartId", "name": "Discount", "autoWidth": true },
            {
                "data": "", "name": "Total Price", "autoWidth": true, "render": function (data, type, full, meta) {
                    return (full.qty * full.part_Price).toFixed(2);
                }
            },
            {
                data: "", data: "Receipt Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.receiptDate != null && full.receiptDate != '') {
                        return convertDateToStrDate(full.receiptDate);
                    }
                    else {
                        return '';
                    }

                }
            },
            {
                data: "", data: "Arrival Date", "autoWidth": true, "render": function (data, type, full, meta) {

                    if (full.arrivalDate != null && full.arrivalDate != '') {
                        return convertDateToStrDate(full.arrivalDate);
                    }
                    else {

                        return '';
                    }

                }
            },
            { "data": "volume", "name": "Weight", "autoWidth": true },

        ],
        "initComplete": function (data) {


            $('#LinesTotalRecord').text(data.aoData.length);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        }

    });
    // SearchData(oTable);
}

function InitialiseDOTable(_url, encid, postatusId) {

    var isaccess = false;
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var count = 0;
    oTable = $("#tblDeliveryOrders").DataTable({
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
        "order": [[0, "asc"]],
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10, // Depending on your requirement, you may set it to a different value
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { InternalOrderid: encid },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var x = 0;
            },

            "dataSrc": function (data) {
                
                //doLength = data.length;
                return data;

            }
        },
        "columnDefs": [
            { orderable: false }
            /*{ "visible": isaccess, "targets": 7 }*/
        ],
        "columns": [
            {
                "data": "delivery_order_no", "name": "DO No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[2] > AccessLevels.NoAccess) {
                        return "<a href='#' class='do-details' data-id=" + full.delivery_order_id + ">" + full.delivery_order_no + " </a> "
                    }
                    else {
                        return full.delivery_order_no
                    }
                }

            },
            {
                "data": "customer_no", "name": "Party", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (UserDefaultData.isbuyer) {
                        return full.companyCode + "-" + full.companyName;
                    }
                    else {
                        return full.customer_no + "-" + full.customer_name;
                    }


                }
            },

            { "data": "sales_order_no", "name": "Sales Order No.", "autoWidth": true },
            { "data": "vessel_name", "name": "Vessel Name", "autoWidth": true },
            {
                "data": "vessel_eta", "name": "Vessel ETA", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.vessel_eta != null) {
                        return moment(new Date(full.vessel_eta)).format('DD/MM/YYYY');
                    }
                    else {
                        return "";
                    }
                }
            },
            { "data": "do_job_no", "name": "Job No Code", "autoWidth": true },
            { "data": "internal_dept", "name": "International Dept Code", "autoWidth": true },
            { "data": "status_desc", "name": "Status", "autoWidth": true },
            { "data": "sales_person_code", "name": "Sales Person Code", "autoWidth": true },
            {
                "data": "", "name": "Attachment", "autoWidth": true, "render": function (data, type, full, meta) {
                    //return '<a href="#" data-id="' + full.delivery_order_id + '" onclick="GetOrderAttachment(' + full.delivery_order_id + ')"><i class="fa fa-paperclip fs-3 me-2" aria-hidden="true"></i></a>';
                    return '<a href="#" data-id="' + full.delivery_order_id + '" data-dono="' + full.delivery_order_no + '" class="downloadAttachments"><i class="fa fa-paperclip fs-3 me-2" aria-hidden="true"></i></a>';
                }
            },
            {
                "data": "", "name": "Actions", "autoWidth": true, "render": function (data, type, full, meta) {
                    //return '<a href="#" style="color: #fa7d7d; pointer - events: none;" data-id="' + full.delivery_order_id + '" onclick="UnAssignDOs(' + full.delivery_order_id + ')">UnAssign DO</i></a>';

                    //return '<a href="#" style="color: #fa7d7d; pointer - events: none;" class="ActionUnassignDO" data-id="' + full.delivery_order_id + '" data-dono="' + full.delivery_order_no + '">Remove</i></a>';
                    if (full.statusid == 3) {
                        return "";
                    }
                    else {
                        return `
             <div style="display: flex; gap: 5px;">
            <button
                type="button"
                class="btn btn-primary btn-sm ActionReleaseDO"
                data-id="${full.delivery_order_id}" 
                data-dono="${full.delivery_order_no}" 
                style="width:84px;">
                Release
            </button>
            <button 
                type="button" 
                class="btn btn-sm btn-danger ActionUnassignDO" 
                data-id="${full.delivery_order_id}" 
                data-dono="${full.delivery_order_no}"
                style="width:84px;">
                Remove
            </button>
        </div>`;
                    }
                }
            },

        ],
        "initComplete": function (data) {


            $('#DOTotalRecord').text(data.aoData.length);


        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        }

    });
    // SearchData(oTable);
}


$(document).on('click', '.downloadAttachments', async function () {
    // Use `$(this)` to reference the clicked element
    var deliveryorderid = $(this).attr('data-id');
    var deliveryorderno = $(this).attr('data-dono');

    GetOrderAttachment(deliveryorderid);
});
function GetOrderAttachment(deliveryOrderId) {

    $('#modalAttachment').modal({ backdrop: 'static', keyboard: false });
    ClearModalFields();

    var gh = 0;
    var count = 0;
    var _url = pathname + "/Orders/GetOrderAttachment";
    var isaccess = false;
    if (slModuleAction[1002] > 3) {
        isaccess = true;
    }

    shipdoctable = $("#tblshipmentDocuments").DataTable({
        "language": {
            "infoFiltered": ""
        },
        "scrollY": '15vh',
        info: false,
        ordering: false,
        paging: false,
        //"scrollCollapse": true,
        //"scrollX": true,
        "processing": false,
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
            "data": { DeliveryOrderId: deliveryOrderId },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var sd = 0;
            },

            "dataSrc": function (data) {
                return data.data;
            }
        },
        "columnDefs": [
            { "visible": false, "targets": 0 }
        ],
        "columns": [
            { "data": "deliveryDocumentId", "name": "", "autoWidth": true },
            {
                "data": "", "name": "No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    return count = count + 1;
                }
            },
            {
                "data": "", "name": "Attachments", "autoWidth": true, "render": function (data, type, full, meta) {
                    return full.documentName;
                }
            },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    //return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnDownloadDO" data-docid ="' + full.deliveryDocumentId + '" data-filename="' + full.documentName + '" href="#" onclick="DownloadAttachment(' + full.deliveryOrderId + ')">Download</a>'
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnDownloadDO" data-docid ="' + full.deliveryDocumentId + '" data-DOid ="' + full.deliveryOrderId + '" data-filename="' + full.documentName + '" href="#">Download</a>'
                }
            }


        ],
        "initComplete": function (data) {
            //$('#deliveryordersCount').text(data.aoData.length);
        },
        "language": {
            "emptyTable": 'There are no attachments linked yet!'
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);

        }
    }
    )

    $('#modalAttachment').modal('show');
}
$(document).on('click', '.btnDownloadDO', function () {
    var deliveryDocumentId = $(this).attr('data-docid');
    var DOId = $(this).attr('data-DOid');
    DownloadAttachment(deliveryDocumentId, DOId)
});

function DownloadAttachment(deliveryDocumentId, DOId) {
    var _docid = $(this).attr('data-docid');
    $.ajax({
        url: pathname + "/Orders/DownloadOrderAttachment",
        data: { documentid: deliveryDocumentId, deliveryId: DOId },
        type: "GET",
        success: function (response) {
            if (response.result === "SUCCESS") {
                var byteCharacters = atob(response.data.base64Data);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], { type: response.data.fileType });

                // Create temporary link to download the file
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = response.data.document_Name; // Set the filename
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                // Display error message
                swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.msg
                });
            }
        },
        error: function (xhr, status, error) {
            // Handle other errors with SweetAlert
            swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again later.'
            });
        }
    });
}

$(document).on('click', '.ActionUnassignDO', async function () {
    // Use `$(this)` to reference the clicked element
    var deliveryorderid = $(this).attr('data-id');
    var deliveryorderno = $(this).attr('data-dono');

    UnAssignDOs(deliveryorderid, deliveryorderno);
});

$(document).on('click', '.ActionReleaseDO', async function () {
    // Use `$(this)` to reference the clicked element
    var deliveryorderid = $(this).attr('data-id');
    var deliveryorderno = $(this).attr('data-dono');
    var internalOrderid = poid;
    ReleaseDOs(deliveryorderid, deliveryorderno, internalOrderid);
});
async function ReleaseDOs(deliveryorderid, deliveryorderno, internalOrderid) {
    if (deliveryorderid != "") {

        const result = await Swal.fire({
            title: 'Released Delivery Order',
            text: `Are you sure you want to Released DO : '${deliveryorderno}'?`,
            icon: '',

            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'No'
            //reverseButtons: true // Optional: switches the position of Yes and No buttons
        });

        // Handle the user's response
        if (result.isConfirmed) {
            // User clicked 'Yes'
            ReleaseDeliveryOrders(deliveryorderid, internalOrderid);
            //console.log(`Delivery Order ID: '${deliveryorderid}' has been unassigned.`);
            // Add your unassign logic here
        } else {
            // User clicked 'No'
            //console.log(`Action canceled for Delivery Order ID: '${deliveryorderid}'.`);
        }
    }
}
async function UnAssignDOs(deliveryorderid, deliveryorderno) {

    if (deliveryorderid != "") {

        const result = await Swal.fire({
            title: 'Unassign Delivery Order',
            text: `Are you sure you want to unassign DO : '${deliveryorderno}'?`,
            icon: '',

            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'No'
            //reverseButtons: true // Optional: switches the position of Yes and No buttons
        });

        // Handle the user's response
        if (result.isConfirmed) {
            // User clicked 'Yes'
            UnAssignDeliveryOrders(deliveryorderid);
            GetLinkedDOcountAndPOlineCount(poid);
            //console.log(`Delivery Order ID: '${deliveryorderid}' has been unassigned.`);
            // Add your unassign logic here
        } else {
            // User clicked 'No'
            //console.log(`Action canceled for Delivery Order ID: '${deliveryorderid}'.`);
        }
    }
}
function UnAssignDeliveryOrders(deliveryorderid) {

    var dfghj = 0;
    $.ajax({
        url: pathname + '/Inbound/UnAssignDOs',
        type: 'POST',
        data: { deliveryOrderid: deliveryorderid },
        success: function (response) {
            //$('#uploadMessage').text(response.message);

            if (response.result) {
                var fghj = 0;
                Swal.fire({
                    text: response.msg,
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
                    //location.reload();
                    InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", poid, postatusid);
                    location.reload();
                }));
            }
            else {
                Swal.fire('Failed', 'Message : ' + response.msg, 'error');
            }


        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            //$('#uploadMessage').text('Unable to upload file.');
        }
    });

}
function ReleaseDeliveryOrders(deliveryorderid, internalOrderid) {
    var dfghj = 0;
    $.ajax({
        url: pathname + '/Inbound/ReleaseDOs',
        type: 'POST',
        data: { deliveryOrderid: deliveryorderid, InternalOrderId: internalOrderid },
        success: function (response) {
            //$('#uploadMessage').text(response.message);

            if (response.result) {
                var fghj = 0;
                Swal.fire({
                    text: response.msg,
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
                    
                    InitialiseDOTable(pathname + "/Inbound/GetDeliveryOrderList", poid, postatusid);
                    location.reload();
                }));
            }
            else {
                Swal.fire('Failed', 'Message : ' + response.msg, 'error');
            }


        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            //$('#uploadMessage').text('Unable to upload file.');
        }
    });
}
function LoadUploadedFilesInTable(files) {
    const uploadedFilesTable = document.getElementById("uploadedFilesTable");

    // Clear the table to avoid duplicate entries
    uploadedFilesTable.innerHTML = "";
    if (files.length === 0) {
        const row = document.createElement("tr");
        const noFileCell = document.createElement("td");
        noFileCell.setAttribute("colspan", 2);
        noFileCell.textContent = "No files selected.";
        row.appendChild(noFileCell);
        uploadedFilesTable.appendChild(row);
        return;
    }
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Create a new row for the uploaded file
        const row = document.createElement("tr");

        // File Name Cell with text-wrap class
        const fileNameCell = document.createElement("td");
        fileNameCell.textContent = file.name;
        fileNameCell.classList.add("text-wrap");

        // Actions Cell
        const actionsCell = document.createElement("td");
        const removeButton = document.createElement("button");
        removeButton.textContent = "Remove";
        removeButton.classList.add("btn", "btn-sm", "btn-danger");

        // Remove file from list when "Remove" button is clicked
        removeButton.addEventListener("click", function () {
            row.remove(); // Remove the row
            updateFileInput(); // Update the file input after removal
        });

        actionsCell.appendChild(removeButton);

        // Append cells to the row
        row.appendChild(fileNameCell);
        row.appendChild(actionsCell);

        // Append the row to the table body
        uploadedFilesTable.appendChild(row);
    }
}
function makeQtyColumnEditable() {
    // Loop through each row in the DataTable and make the Qty column editable
    $('#tblpartlines tbody tr').each(function () {
        var row = $(this);

        // Find the "Qty" column (4th column, index 3)
        var qtyCell = row.find('td').eq(3); // 4th column (index 3)
        var orderedQuantity = qtyCell.text(); // Get the current "Qty" value

        // Store the original value in data-original attribute and replace with input field
        qtyCell.html(`
            <input type="number" class="editable-qty form-control form-control-sm fs-7 text-dark" 
                   value="${orderedQuantity}" 
                   min="0" 
                   max="${orderedQuantity}" 
                   style="width: 100%; box-sizing: border-box;" 
                   data-max="${orderedQuantity}" 
                   data-original="${orderedQuantity}" /> <!-- Storing original value here -->
        `);
    });


}
function validateNumber(value) {
    // Check if the value is a valid number
    if (isNaN(value) || value === '') {
        return false;
    }

    // Check if the value is an integer
    if (Number.isInteger(parseFloat(value))) {
        return true;
    }

    // Check if the value is a float (not an integer)
    if (parseFloat(value) === parseFloat(value).toFixed(2) && value.includes('.')) {
        return true;
    }

    // Default return (for other types of numbers, i.e., double-like behavior)
    return false;
}

function getCheckedRowsData() {
    var checkedRows = [];

    // Use DataTable API to get all rows, not just the current page
    Rtable.rows({ search: 'applied' }).every(function () {
        var table = $('#yourTableId').DataTable();
        var row = this.node();
        var checkbox = $(row).find('.row-select');

        if (checkbox.is(':checked')) {
            // Extract internalOrderLineId from the 2nd column
            var internalOrderLineId = $(row).children(':eq(0)').children(':first').attr('data-id');
            //var internalOrderLineId = $(row).find('td').eq(1).text(); // Assuming internalOrderLineId is in the 2nd column
            //var internalOrderLineId2 = table.cell(row, 0).data(); 
            // Check if there is an input field in the "Qty" column (4th column)
            var value = 0;
            var qty = $(row).find('td').eq(3).text();
            if (validateNumber(qty)) {
                value = qty;
            }
            else {
                var qtyCell = $(row).find('td').eq(3);
                value = qtyCell.find('input').val(); // Get value of the input inside the "Qty" column
            }


            checkedRows.push({
                internalOrderLineId: internalOrderLineId,
                qty: value
            });
        }
    });

    return checkedRows;
}



function LoadAttachments(_url, id) {
    var isaccess = false;
    shipdoctable = $("#tbldodocs").DataTable({
        "language": {
            "infoFiltered": ""
        },
        "scrollY": '15vh',
        info: false,
        ordering: false,
        paging: false,
        //"scrollCollapse": true,
        //"scrollX": true,
        "processing": false,
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
            "data": { internalorderid: id },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {
                var sd = 0;
            },

            "dataSrc": function (data) {
                return data.data;
            }
        },
        "columnDefs": [
            { "visible": false, "targets": 0 },
            { "visible": isaccess, "targets": 2 }
        ],
        "columns": [
            { "data": "shipmentDocumentId", "name": "", "autoWidth": true },
            {
                "data": "", "name": "Attachments", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        return "<a href='#'class='shipment-docc'  data-id=" + full.shipmentDocumentId + ">" + full.documentName + "</a>"

                    }
                    else {
                        return full.documentName;

                    }
                }
            },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 Doc-delete " data-docid ="' + full.shipmentDocumentId + '" data-filename="' + full.documentName + '" href="#">Remove</a>'
                }
            }


        ],
        "initComplete": function (data) {
            //$('#deliveryordersCount').text(data.aoData.length);
        },
        "language": {
            "emptyTable": 'There are no attachments linked yet!'
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);

        }
    }
    )
};
function DownloadOutboundAttachment(documentId) {
    $.ajax({
        url: pathname + "/Inbound/DownloadshipmentAttachement",
        data: { documentid: documentId, shipmentid: inboundid },
        type: "GET",
        success: function (response) {
            if (response.result) {

                var byteCharacters = atob(response.base64Data);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], { type: response.data.fileType });

                // Create temporary link to download the file
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = response.data.document_Name; // Set the filename
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                // Display error message
                swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.msg
                });
            }
        },
        error: function (xhr, status, error) {
            // Handle other errors with SweetAlert
            swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again later.'
            });
        }
    });
}
function initializeDataTable() {
    $("#tblOrderLines").DataTable({
        data: [], // Start with an empty dataset
        columns: [
            { title: "Line No.", data: "Line No." },
            { title: "Item No.", data: "Item No." },
            { title: "Description", data: "Description" },
            { title: "Qty", data: "Qty" },
            { title: "UOM", data: "UOM" },
            { title: "Rec Qty", data: "Rec Qty" },
            { title: "Unit Price", data: "Unit Price" },
            { title: "Discount", data: "Discount" },
            { title: "Total Price", data: "Total Price" },
            {
                data: "receiptDate", data: "Receipt Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    return new Date(dataObject.receiptDate).toLocaleDateString();
                }
            },
            {
                data: "arrivalDate", data: "Arrival Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    new Date(dataObject.arrivalDate).toLocaleDateString()
                }
            },
            { title: "volume", data: "Weight" }
        ],
        destroy: true // Ensures table re-initializes cleanly
    });
}

// Function to populate a DataTable
function populateDataTable(dataObjects) {
    initializeDataTable();
    // Transform dataObjects into rows for the table
    const tableRows = dataObjects.map(dataObject => ({
        "Line No.": dataObject.lineNo,
        "Item No.": dataObject.salesPartId,
        "Description": dataObject.partName,
        "Qty": dataObject.qty,
        "UOM": dataObject.uom,
        "Rec Qty": dataObject.qty_Rec,
        "Unit Price": dataObject.part_Price,
        "Discount": "-", // Placeholder or calculation
        "Total Price": (dataObject.qty * dataObject.part_Price).toFixed(2),
        "Receipt Date": new Date(dataObject.receiptDate).toLocaleDateString(),
        "Arrival Date": new Date(dataObject.arrivalDate).toLocaleDateString(),
        "Weight": dataObject.volume || "-"
    }));

    // Add data to the table
    const dataTable = $("#tblOrderLines").DataTable();
    dataTable.clear(); // Clear existing data
    dataTable.rows.add(tableRows); // Add new data
    dataTable.draw(); // Redraw the table
}

//#endregion Fill Data

//#region Upload DO doc
//function UploadDocDO() {
//    var fileInput = $('#fileInputdo')[0];
//    var file = fileInput.files[0];
//    var shipmentId = $('#txtpono').attr('data-id');

//    if (file && shipmentId) {

//        if (isValidFileType(file.name)) {
//            var formData = new FormData();
//            formData.append('formFile', file);
//            formData.append('shipmentId', shipmentId);

//            $.ajax({
//                url: pathname + '/Inbound/UploadAttachments',
//                type: 'POST',
//                data: formData,
//                contentType: false,
//                processData: false,
//                success: function (response) {
//                    $('#uploadMessage').text(response.message);

//                    if (response.success) {
//                        Swal.fire({
//                            text: "File successfully uploaded!",
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

//                            $('#fileInputdo').val('');
//                            LoadAttachments(pathname + "/Inbound/GetInboundShipmentDocuments", shipmentId);

//                        }));

//                    }
//                    else {
//                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
//                    }


//                },
//                error: function (xhr, status, error) {
//                    console.error('Error:', error);
//                    $('#uploadMessage').text('Unable to upload file.');
//                }
//            });
//        }
//        else {
//            Swal.fire('Failed', 'Oops! This file type is not supported.Try another !', 'error');
//            $('#uploadMessage').text('Oops! This file type is not supported.Try another !');
//        }
//    } else {
//        $('#uploadMessage').text('No file is selected, Please choose file!');
//    }
//}
//function UploadDoc() {
//    var fileInput = $('#fileInputdo')[0];
//    var file = fileInput.files[0];
//    var shipmentId = $('#txtpono').attr('data-id');;

//    if (file && shipmentId) {

//        if (isValidFileType(file.name)) {



//            var formData = new FormData();
//            formData.append('formFile', file);
//            formData.append('shipmentId', shipmentId);

//            $.ajax({
//                url: pathname + '/Inbound/UploadAttachments',
//                type: 'POST',
//                data: formData,
//                contentType: false,
//                processData: false,
//                success: function (response) {
//                    $('#uploadMessage').text(response.message);

//                    if (response.success) {
//                        Swal.fire({
//                            text: "File successfully uploaded!",
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

//                            $('#fileInput').val('');
//                            LoadAttachments(pathname + "/Inbound/GetInboundShipmentDocuments", shipmentId);

//                        }));

//                    }
//                    else {
//                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
//                    }


//                },
//                error: function (xhr, status, error) {
//                    console.error('Error:', error);
//                    $('#uploadMessage').text('Unable to upload file.');
//                }
//            });
//        }
//        else {
//            Swal.fire('Failed', 'Oops! This file type is not supported.Try another !', 'error');
//            $('#uploadMessage').text('Oops! This file type is not supported.Try another !');
//        }
//    } else {
//        $('#uploadMessage').text('No file is selected, Please choose file!');
//    }
//}
function isValidFileType(fileName) {
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'pdf', 'txt'];
    const fileExtension = fileName.split('.').pop().toLowerCase();
    return allowedExtensions.includes(fileExtension);
}

//#endregion Upload DO doc

function MergePO() {
    return new Promise((resolve, reject) => {
        if (quotationid > 0) {
            EncryptId(quotationid)
                .then(encid => {
                    return $.ajax({
                        url: pathname + '/Inbound/InsertPoFromEsupplierToLogiLink',
                        type: 'POST',
                        data: { quotationid: encid },
                        success: function (response) {
                            console.log(response.msg);
                            if (response.data != null) {
                                var dta = response.data.split('|');
                                $('#txtpono').attr('data-id', dta[0]);
                                $('#txtpono').attr('data-inboundid', dta[1]);
                                poid = response.data
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error("Error while fetching shipment details: ", error);
                        }
                    });
                })
                .catch(error => {
                    console.error("Error in encryption or subsequent process: ", error);
                });
        }
    });
}