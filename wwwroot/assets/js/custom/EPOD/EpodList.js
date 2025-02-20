var oTable = null;
var oShipmentTable = null;
var selectedDeliveryOption = '';
var selectedDeliveryOptioncopy = '';
var fShipmentNo, fOrderNo, fSite, fCustomer, fCurrency, fCoordinator, fStatus, fOrderFromDate, fOrderToDate, fordertype, fTransportType, fdeliveryIn, fromDate, toDate, shipmentID;
$(document).ready(function () {
    $('#nav_EPOD').addClass("show");
    $('#menuEpodList .menu-link').addClass("active");
    
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });
    $('#dtOrderToDate').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y", minDate: "today"
    });
    $('#fromDate').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });
    $('#toDate').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"
    });
    $('#btnOpenStatus').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Open Status").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnOpenStatus';   
        selectedDeliveryOptioncopy = 'btnOpenStatus';   
        clearFilters();
        ApplyFilter();
        $('#dropdownMenuButton').dropdown('toggle');
        
    });

    $('#btnEpodList').click(function (e) {
        PrintEpodList();

    });
    $(document).on('change', '#fromDate, #toDate', function (e) {
        var isValid = validateDates();
        if (!isValid) {
            Swal.fire('Validation Error !', 'Invalid date range: From Date is greater than To Date.', 'error');
            $("#fromDate").val('').trigger('change');;
            $("#toDate").val('').trigger('change');;
        }
    })
    $('#btnInitialReceiptCompleted').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Initial Receipt Completed").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnInitialReceiptCompleted';
        selectedDeliveryOptioncopy = 'btnInitialReceiptCompleted';
        clearFilters();
        ApplyFilter();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btnFinalReceiptCompleted').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Final Receipt Completed").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnFinalReceiptCompleted';
        selectedDeliveryOptioncopy = 'btnFinalReceiptCompleted';
        clearFilters();
        ApplyFilter();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    
    $(document).on('click', '.epod-data', function (e) {
        e.stopPropagation();
        var target = document.querySelector("#baseCard");
        var blockUI = new KTBlockUI(target, {
            overlayClass: "bg-white bg-opacity-10"
        });
        blockUI.block();
        var overlay = document.querySelector('.blockui-overlay');
        if (overlay) {
            overlay.style.position = 'fixed';
            $('.blockui-overlay').css('z-index', '2000');

        }
        var ShipmentId = e.target.attributes['data-id'].nodeValue;
        if (ShipmentId > 0) {
            getCurrentLocation()
                .then((location) => {
                    if (location) {
                        blockUI.release();
                        blockUI.destroy();
                        viewDetails(ShipmentId, location.latitude, location.longitude);
                        //console.log('Latitude:', location.latitude, 'Longitude:', location.longitude);
                    } else {
                        blockUI.release();
                        blockUI.destroy();
                        Swal.fire('Permission Error', 'Location access was denied. Please enable location services and try again.', 'error')
                        console.log('Location access was denied. Please enable location services and try again.');
                    }
                })
                .catch((error) => {
                    blockUI.release();
                    blockUI.destroy();
                    console.error('Error occurred:', error);
                    Swal.fire('Permission Error', 'An error occurred while retrieving location. Please try again later.', 'error');

                });
        }
        else {
            blockUI.release();
            blockUI.destroy();
            Swal.fire('Error', 'Oops, something went wrong!', 'error');
        }

    });
    $('#btnApply_Filter').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Quick Filter");
        selectedDeliveryOption = '';
        selectedDeliveryOptioncopy
        ApplyFilter();
        resetDropdownLabelColor();
    });
    function resetDropdownLabelColor() {
        $('#dropdownMenuButton').text("Quick Filter").removeClass('btn-primary').addClass('btn-secondary');
    }

    InitialiseTable(pathname + "/EPOD/GetEpodListTable");
    
    var myCollapsible = document.getElementById('kt_accordion_filter')
    myCollapsible.addEventListener('hidden.bs.collapse', function () {
        SetFilterVars();
        /*AddTagFilter();*/
    })
    myCollapsible.addEventListener('show.bs.collapse', function () {
        $('#tagscontainer').hide();
    })

    //$('#btnPrintShipmentOrder').click(function () {
    //    $('#printModal').modal('show');
    //});

    $('#confirmPrint').click(function () {
        PrintShipmentOrder();
        $('#printModal').modal('hide');
    });

    $('#previewbtn').click(function () {
        PreviewDocument();
        $('#printModal').modal('hide');
    });

    SetTransportTypeDropdown();
    SetStatusDropdown();
    SetCustomerDropdown();
});
function getCurrentLocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const coordinates = {
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude
                    };
                    resolve(coordinates);
                },
                (error) => {
                    if (error.code === error.PERMISSION_DENIED) {
                        resolve(false);
                    } else {
                        reject(error);
                    }
                }
            );
        } else {
            reject(new Error("Geolocation is not supported by this browser."));
        }
    });
}

// Usage example:


function clearFilters() {
    $('#txtShipmentNo').val('');
    $('#selcustomer').val('').trigger('change');
    $('#selTransportType').val('').trigger('change');
    $('#selStatus').val('').trigger('change');
    $('#txtJobNo').val('');
    $('#txtVessel').val('');
    $('#fromDate').val('');
    $('#toDate').val('');

    //resetDropdownLabelColor();
}
function SetStatusDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetModuleStatuses", {"moduleId":1 }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc  + '</option>';
        }
    }
    $('#selStatus')[0].innerHTML = options;
    Outboundstatus = OptionData.data.reduce((acc, item) => {
        acc[item.status_desc.toLowerCase()] = item.statusid;
        return acc;
    }, {});


}
function SetTransportTypeDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetTransportTypeList", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].transport_type_id + '>' + OptionData.data[i].transport_type_description + '</option>';
        }
    }
    $('#selTransportType')[0].innerHTML = options;


}
function SetCustomerDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetCustomers", {}, true);
    var options = '<option></option>';
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].customerId + ' data-company=' + OptionData.data[i].companyId + '>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';

        }
    }
    $('#selcustomer')[0].innerHTML = options;
}

function AddTagFilter() {
    $(".bootstrap-tagsinput").removeClass('cfilterHide');
    $(".tagify").tagsinput('removeAll');
    if (fShipmentNo != undefined && fShipmentNo != '') {
        $('#ShipmentNotags').tagsinput('add', { id: 'tagShipmentNotags', label: fShipmentNo });
        var data = $('#ShipmentNotags').prev();
        $('#ShipmentNotags').prev().addClass('cfilterShow');
        data.children().prepend("<b>Shipment No. : </b>");
    }
    if (fTransportType != undefined && fTransportType != '') {
        $('#TransportTypetags').tagsinput('add', { id: 'tagTransportTypetags', label: fTransportType });
        var data = $('#TransportTypetags').prev();
        data.children().prepend("<b>Transport Type : </b>");
    }
    if (fCustomer != undefined && fCustomer != '') {
        $('#Customertags').tagsinput('add', { id: 'tagCustomertags', label: fCustomer });
        var data = $('#Customertags').prev();
        data.children().prepend("<b>Customer : </b>");
    }
    if (fCustomerCode != undefined && fCustomerCode != '') {
        $('#CustomerCodetags').tagsinput('add', { id: 'tagCustomerCodetags', label: fCustomerCode });
        var data = $('#CustomerCodetags').prev();
        data.children().prepend("<b>Customer Code : </b>");
    }
    if (fVessel != undefined && fVessel != '') {
        $('#Vesseltags').tagsinput('add', { id: 'tagVesseltags', label: fVessel });
        var data = $('#Vesseltags').prev();
        data.children().prepend("<b>Vessel : </b>");
    }
    if (fStatus != undefined && fStatus != '') {
        $('#Statustags').tagsinput('add', { id: 'tagStatustags', label: fStatus });
        var data = $('#Statustags').prev();
        data.children().prepend("<b>Status : </b>");
    }
    if (fJobNo != undefined && fJobNo != '') {
        $('#JobNotags').tagsinput('add', { id: 'tagJobNotags', label: fJobNo });
        var data = $('#JobNotags').prev();
        data.children().prepend("<b>Job No. : </b>");
    }
    
}
function getValidcustId() {

    let selectedValue = $('#selcustomer').val();
    let selectedDataCompany = $('#selcustomer option:selected').data('company');
    if (selectedValue > 0) {
        return selectedValue;
    }
    if (selectedDataCompany > 0) {
        return selectedDataCompany;
    }
    return null;
}
function SetFilterVars() {
    var queryList = '';

    var fShipmentNo = $('#txtShipmentNo').val();
    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();

    //var _selVal = $("#selcustomer option:selected");
    //var fcustomer = _selVal.val();
    //if (fcustomer != '') queryList += "&customerid=" + fcustomer.trim();

    var _selVal = $("#selcustomer option:selected");
    var fcustomer = _selVal.val();
    var companyid = _selVal.data('company') ?? 0;
    if (fcustomer != '' && fcustomer != '0') queryList += "&customerid=" + fcustomer.trim();
    else queryList += "&companyid=" + companyid;

    _selVal = $("#selTransportType option:selected");
    var fTransportType = _selVal.val();
    if (fTransportType != '') queryList += "&transportType=" + fTransportType.trim();

    var fStatus = '';
    if (selectedDeliveryOption === "btnOpenStatus") {
        fStatus = Outboundstatus['ready to ship']; 
        selectedDeliveryOption = '';
    } else if (selectedDeliveryOption === "btnInitialReceiptCompleted") {
        //fStatus = 12;
        //selectedDeliveryOption = '';
        fStatus = Outboundstatus['goods verified'];
        selectedDeliveryOption = '';
    } else if (selectedDeliveryOption === "btnFinalReceiptCompleted") {
        //fStatus = 10;
        //selectedDeliveryOption = '';
        fStatus = Outboundstatus['completed'];
        selectedDeliveryOption = '';
    }
    else {
        var _selVal = $("#selStatus option:selected");
        var fStatusFromDropdown = _selVal.val();
        if (fStatusFromDropdown !== '') {
            fStatus = fStatusFromDropdown;
            selectedDeliveryOption = '';
        }
    }
    if (fStatus !== '') queryList += "&status=" + fStatus;

    
    var fJobNo = $('#txtJobNo').val();
    if (fJobNo != '') queryList += "&jobNo=" + fJobNo.trim();


    var fVessel = $('#txtVessel').val();
    if (fVessel != '') queryList += "&vesselName=" + fVessel.trim();


    var fromDate = $('#fromDate').val();
    if (fromDate != '' && fromDate != undefined) queryList += "&fromDate=" + fromDate;


    var toDate = $('#toDate').val();
    if (toDate != '' && toDate != undefined) queryList += "&toDate=" + toDate;

    return queryList;
}



function ApplyFilter() {
    var queryList = SetFilterVars();

    if (queryList.length > 0) {

        InitialiseTable(pathname + "/EPOD/GetEpodListTable?" + queryList.trim('&').trim().substring(1));
    }
    else {

        InitialiseTable(pathname + "/EPOD/GetEpodListTable");
    }
}

var SearchData = (oTable) => {
    const filterSearch = document.querySelector('[data-kt-filter="search"]');
    let debounceTimer; // Initialize a timer variable

    filterSearch.addEventListener('keyup', function (e) {
        clearTimeout(debounceTimer); // Clear the timer if it exists
        debounceTimer = setTimeout(() => { // Set a new timer
            oTable.search(e.target.value).draw();
        }, 1000); // 1000 milliseconds = 1 second
    });
}

var statusMap = {
    "COMPLETED": 10,
};


function InitialiseTable(_url) {
    var isaccess = false;
    var totalrecords = 0;

    if (slModuleAction[1] > AccessLevels.Write) {
        isaccess = true;
    }
    var rdVal = Math.floor(Math.random() * 100) + 1;
    oShipmentTable = $("#tblData").DataTable({
        "language": {
            "infoFiltered": ""
        },
        //"scrollY": '40vh',
        //"scrollCollapse": true,
        //"scrollX": true,
        "processing": true,
        "serverSide": true,
        "filter": true,
        "orderMulti": false,
        //"order": [[0, "desc"]],
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
                "data": "order_no", "name": "Shipment No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        return "<a href='#'  data-id=" + full.shipmentid + " class='epod-data'>" + full.order_no + "</a>"

                    }
                    else {
                        return full.order_no;
                    }
                }
            },
            { "data": "shipment_statusdesc", "name": "Status", "autoWidth": true },
            {
                "data": "order_no", "name": "Shipment Order", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        return "<a href='#' data-id=" + full.shipmentid + " onclick='ModalPage(" + full.shipmentid + ")'><i class='fa fa-paperclip text-primary'></i></a>";
                    } else {
                        return "<i class='fa fa-paperclip text-primary'></i>";
                    }
                }
            },
            {
                "data": "order_no",
                "name": "Good Returns Report",
                "autoWidth": true,
                "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        var isEnabled = full.shipment_statusid === statusMap["COMPLETED"];
                        var finalreciptMark = full.finalReceiptExported;
                        if (isEnabled && finalreciptMark==1) {
                            return "<a href='#' data-id=" + full.shipmentid + " onclick='GenerateGoodsReturnReport(" + full.shipmentid + ")'><i class='fa fa-paperclip text-primary'></i></a>";
                        } else {
                            return "<i class='fa fa-paperclip text-secondary'></i>";
                        }
                    } else {
                        return "<i class='fa fa-paperclip text-secondary'></i>";
                    }
                }
            },

            {
                "data": "delivery_date", "name": "Delivery Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (data != null) {
                        return moment(data).format('DD/MM/YYYY HH:mm:ss');
                    } else {
                        return "";
                    }
                }
            },
            {
                "data": "", "name": "Party", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (UserDefaultData.isbuyer) {
                        return full.companyCode + "-" + full.companyName;
                    }
                    else {
                        return full.cust_code + "-" + full.cust_name;
                    }
                   
                }
            },
            
            { "data": "jobno", "name": "Job No. Code", "autoWidth": true },
            { "data": "vessel_name", "name": "Vessel Name", "autoWidth": true },
            
            { "data": "anchorage_description", "name": "Anchorage", "autoWidth": true },
            { "data": "agent", "name": "Agent", "autoWidth": true },
            { "data": "supply_boat", "name": "Supply Boat", "autoWidth": true },
            { "data": "transport_type_description", "name": "Transport Type", "autoWidth": true },
            { "data": "driver_name", "name": "Driver", "autoWidth": true },
            { "data": "vehicle_no", "name": "Truck No", "autoWidth": true },
            { "data": "boarding_Officer_Name", "name": "Boarding Officer", "autoWidth": true },
            
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
    SearchData(oShipmentTable);
};
function viewDetails(shipmentId, latitude, longitude) {
    var target = document.querySelector("#baseCard");
    var blockUI = new KTBlockUI(target, {
        overlayClass: "bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
        $('.blockui-overlay').css('z-index', '2000');

    }
    $.ajax({
        url: pathname+'/EPOD/GetSessionValues',
        type: 'POST',
        dataType: 'json',
        data: { shipmentId: shipmentId },
        success: function (response) {
            blockUI.release();
            blockUI.destroy();
            if (response.success) {
                ViewEpodDetails(response.responseUrl, latitude, longitude);
                
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Oops, something went wrong please contact support !'
                });
            }
        },
        error: function (xhr, status, error) {
            blockUI.release();
            blockUI.destroy();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request.'
            });
        }
    });
}
function ViewEpodDetails(url, longitude, latitude) {
    // Create a form element
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = EpodDetailUrl // Adjust action and controller

    // Function to create a hidden input field
    const createHiddenInput = (name, value) => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        return input;
    };

    // Append hidden inputs to the form
    form.appendChild(createHiddenInput('url', url));
    form.appendChild(createHiddenInput('longitude', longitude));
    form.appendChild(createHiddenInput('latitude', latitude));

    // Add the form to the document and submit it
    document.body.appendChild(form);
    form.submit();
}
// fucntion for Generate Goods Return Report
function GenerateGoodsReturnReport(shipmentid) {
    
    var target = document.querySelector("#baseCard");
    var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }
    $.ajax({
        type: 'POST',
        url: pathname + "/EPOD/PrintGoodsReturnReport",

        data: {
            _shipmentid: shipmentid
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
    })
};
function ModalPage(Shipmentid) {
    shipmentID = Shipmentid
    $('#printModal').modal('show');
}
function PrintShipmentOrder() {
    var target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }
    let isCheckboxChecked = $('#flexSwitchCheckDefault').is(':checked');

    $.ajax({
        type: 'POST',
        url: pathname + "/Outbound/PrintShipmentOrders",
        data: {
            _shipmentId: shipmentID,
            printWithAllOrders: isCheckboxChecked
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
function PreviewDocument() {
    let target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    let overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }
    let isCheckboxChecked = $('#flexSwitchCheckDefault').is(':checked');

    $.ajax({
        type: 'POST',
        url: pathname + "/Outbound/PrintShipmentOrders",
        data: {
            _shipmentId: shipmentID,
            printWithAllOrders: isCheckboxChecked
        },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            console.log(data)
            var filename = "";
            var disposition = xhr.getResponseHeader('Content-Disposition');
            if (disposition && disposition.indexOf('attachment') !== -1) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches !== null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }
            var blob = new Blob([data], { type: 'application/pdf' });
            var url = window.URL.createObjectURL(blob);
            blockUI.release();
            blockUI.destroy();
            var newTab = window.open(url, '_blank');
            if (newTab) {
                // Set the download attribute to force filename in some browsers
                newTab.document.title = filename; // Set title of new tab (optional)
            } else {
                console.log('Please allow popups for this site'); // Handle popup blocker
            }
        },
        error: function (data, status, xhr) {
            blockUI.release();
            blockUI.destroy();
            console.log("Error: " + error);
        }
    });

}
//function getPaginationInfo() {
//    var table = $('#tblData').DataTable();
//    var pageInfo = table.page.info();

//    // Pagination information
//    var currentPage = pageInfo.page;        // Current page index (0-based)
//    var pageSize = pageInfo.length;         // Records per page
//    var startRecord = pageInfo.start;       // The index of the first record on the current page
//    var totalRecords = pageInfo.recordsTotal; // Total number of records
//    var totalPages = pageInfo.pages;        // Total number of pages

//    var orderInfo = table.order();  // Array containing sorting details [ [columnIndex, sortDirection], ... ]

//    var sortingColumnIndex = orderInfo.length > 0 ? orderInfo[0][0] : null;  // Index of the sorted column
//    var sortingOrder = orderInfo.length > 0 ? orderInfo[0][1] : null;        // Sorting order: 'asc' or 'desc'

//    var sortingColumnName = sortingColumnIndex !== null ? table.column(sortingColumnIndex).header().textContent : null;

//    return {
//        currentPage: currentPage,
//        pageSize: pageSize,
//        startRecord: startRecord,
//        totalRecords: totalRecords,
//        totalPages: totalPages,
//        sortingColumnIndex: sortingColumnIndex,  // Index of the sorted column
//        sortingColumnName: sortingColumnName,    // Name of the sorted column
//        sortingOrder: sortingOrder               // Sorting order (asc or desc)
//    };
//}

function getPaginationInfo() {
    var table = $('#tblData').DataTable();
    var pageInfo = table.page.info();

    // Pagination information
    var currentPage = pageInfo.page;        // Current page index (0-based)
    var pageSize = pageInfo.length;         // Records per page
    var startRecord = pageInfo.start;       // The index of the first record on the current page
    var totalRecords = pageInfo.recordsTotal; // Total number of records
    var totalPages = pageInfo.pages;        // Total number of pages

    var orderInfo = table.order();  // Array containing sorting details [ [columnIndex, sortDirection], ... ]

    var sortingColumnIndex = orderInfo.length > 0 ? orderInfo[0][0] : null;  // Index of the sorted column
    var sortingOrder = orderInfo.length > 0 ? orderInfo[0][1] : null;        // Sorting order: 'asc' or 'desc'

    // Retrieve the data object to match the sorted column with the actual data key (instead of name in DataTables)
    var ajaxData = table.settings()[0].aoColumns.map(function (col) {
        return col.data;
    });

    var sortingColumnNameFromData = sortingColumnIndex !== null ? ajaxData[sortingColumnIndex] : null;

    return {
        currentPage: currentPage,
        pageSize: pageSize,
        startRecord: startRecord,
        totalRecords: totalRecords,
        totalPages: totalPages,
        sortingColumnIndex: sortingColumnIndex,  // Index of the sorted column
        sortingColumnName: sortingColumnNameFromData,  // Data field name from actual data source
        sortingOrder: sortingOrder               // Sorting order (asc or desc)
    };
}

function validateDates() {
    var fromDate = $("#fromDate").val();
    var toDate = $("#toDate").val();

    if (!fromDate || !toDate) {
        return true;
    }

    var from = convertToDate(fromDate);
    var to = convertToDate(toDate);

    if (isNaN(from.getTime()) || isNaN(to.getTime())) {
        return false;
    }

    if (from > to) {
        return false;
    }

    return true;
}
function FilterVarsForPrintEPOD() {
    var queryList = '';
    var pagination = getPaginationInfo();

    var fShipmentNo = $('#txtShipmentNo').val();
    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();
    queryList += "&skip=" + pagination.startRecord;
    queryList += "&length=" + pagination.pageSize;
    queryList += "&sortColumn=" + pagination.sortingColumnName;  // Or use sortingColumnIndex if you prefer
    queryList += "&sortOrder=" + pagination.sortingOrder;
    var _selVal = $("#selcustomer option:selected");
    var fcustomer = _selVal.val();
    if (fcustomer != '') queryList += "&customerid=" + fcustomer.trim();
    let fcustomerName = _selVal.text();
    if (fcustomerName != '') queryList += "&customername=" + fcustomerName.trim();

    _selVal = $("#selTransportType option:selected");
    var fTransportType = _selVal.val();
    if (fTransportType != '') queryList += "&transportType=" + fTransportType.trim();

    let fTransportName = _selVal.text();
    if (fTransportName != '') queryList += "&transportName=" + fTransportName.trim();


    var fStatus = '';
    var qStatusText = '';
    var fStatusText = '';
    if (selectedDeliveryOptioncopy === "btnOpenStatus") {
        fStatus = Outboundstatus['ready to ship'];
        qStatusText = 'READY TO SHIP';
        selectedDeliveryOptioncopy = '';
    } else if (selectedDeliveryOptioncopy === "btnInitialReceiptCompleted") {
        //fStatus = 12;
        //selectedDeliveryOption = '';
        fStatus = Outboundstatus['goods verified'];
        qStatusText = 'GOODS VERIFIED';
        selectedDeliveryOptioncopy = '';
    } else if (selectedDeliveryOptioncopy === "btnFinalReceiptCompleted") {
        //fStatus = 10;
        //selectedDeliveryOption = '';
        fStatus = Outboundstatus['completed'];
        qStatusText = 'COMPLETED';
        selectedDeliveryOptioncopy = '';
    }
    else  {
        var _selVal = $("#selStatus option:selected");
        var fStatusFromDropdown = _selVal.val();
        var fStatusFromDropdownText = _selVal.text();
        if (fStatusFromDropdown !== '' || fStatusFromDropdownText !== '') {
            fStatus = fStatusFromDropdown;
            fStatusText = fStatusFromDropdownText;
            selectedDeliveryOptioncopy = '';
        }
    }
    if (fStatus !== '') queryList += "&status=" + fStatus;
    if (fStatusText !== '') queryList += "&statusText=" + fStatusText;
    if (qStatusText !== '') queryList += "&quickStatus=" + qStatusText;

    var fJobNo = $('#txtJobNo').val();
    if (fJobNo != '') queryList += "&jobNo=" + fJobNo.trim();


    var fVessel = $('#txtVessel').val();
    if (fVessel != '') queryList += "&vesselName=" + fVessel.trim();


    var fromDate = $('#fromDate').val();
    if (fromDate != '' && fromDate != undefined) queryList += "&fromDate=" + fromDate;


    var toDate = $('#toDate').val();
    if (toDate != '' && toDate != undefined) queryList += "&toDate=" + toDate;

    return queryList;
}
function PrintEpodList() {
    var target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }

    var _url = "";
    var queryList = FilterVarsForPrintEPOD();
    if (queryList.length > 0) {

        _url = (pathname + "/EPOD/PrintEpodList?" + queryList.trim('&').trim().substring(1));
    }
    else {
        _url = (pathname + "/EPOD/PrintEpodList");
    }
    //pathname + "/Orders/PrintDeliveryOrderList?" + queryList.trim('&').trim().substring(1),
    $.ajax({
        type: 'POST',
        url: _url,
        data: {},
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

            var blob = new Blob([data], { type: 'application/pdf' });
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







   

   



