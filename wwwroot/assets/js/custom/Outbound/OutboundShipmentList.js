var oTable = null;
var oShipmentTable = null; var selectedDeliveryOption = ''; var selectedDeliveryOptioncopy = '';
var fShipmentNo, fOrderNo, fSite, fCustomer, fCurrency, fCoordinator, fStatus, fOrderFromDate, fOrderToDate, fordertype, fTransportType, fdeliveryIn, fromDate, toDate;
$(document).ready(function () {
    $('#nav_OutboundShipments').addClass("show");
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
    $('#btnTodayDelivery').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Today's Delivery").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnTodayDelivery';
        selectedDeliveryOptioncopy = 'btnTodayDelivery';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btn3DaysDelivery').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Next 3 Days Delivery").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btn3DaysDelivery';
        selectedDeliveryOptioncopy = 'btn3DaysDelivery';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btnReadyToShip').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Ready to Ship").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnReadyToShip';
        selectedDeliveryOptioncopy = 'btnReadyToShip';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btnDraftShipment').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Draft Shipment Order").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnDraftShipment';
        selectedDeliveryOptioncopy = 'btnDraftShipment';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btnApply_Filter').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Quick Filter");
        selectedDeliveryOption = '';
        selectedDeliveryOptioncopy = '';
        ApplyFilter();
        resetDropdownLabelColor();
    });
    $(document).on('change', '#fromDate, #toDate', function (e) {
        var isValid = validateDates();
        if (!isValid) {
            Swal.fire('Validation Error !', 'Invalid date range: From Date is greater than To Date.', 'error');
            $("#fromDate").val('').trigger('change');;
            $("#toDate").val('').trigger('change');;
        }
    })

    // Added by Gaurav
    $('#btnPrintOutBound').click(function (e) {
        console.log("Inside the Click event")
        PrintOutBoundList();
    });
    function resetDropdownLabelColor() {
        $('#dropdownMenuButton').text("Quick Filter").removeClass('btn-primary').addClass('btn-secondary');
    }

    InitialiseTable(pathname + "/Outbound/GetOutboundShipmentTable");
    
    var myCollapsible = document.getElementById('kt_accordion_filter')
    myCollapsible.addEventListener('hidden.bs.collapse', function () {
        SetFilterVars();
        /*AddTagFilter();*/
    })
    myCollapsible.addEventListener('show.bs.collapse', function () {
        $('#tagscontainer').hide();
    })
    
    SetTransportTypeDropdown();
    SetStatusDropdown();
    SetCustomerDropdown();
});

function clearFilters() {
    $('#txtShipmentNo').val('');
    $('#selcustomer').val('').trigger('change');
    $('#selTransportType').val('').trigger('change');
    $('#selStatus').val('').trigger('change');
    $('#txtJobNo').val('');
    $('#txtVessel').val('');
    $('#fromDate').val('');
    $('#toDate').val('');
   // resetDropdownLabelColor();
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
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].customerId + ' data-company=' + OptionData.data[i].companyId+'>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';
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
    //if (fOrderFromDate != "" && fOrderToDate != "") {
    //    $('#OrderDatetags').tagsinput('removeAll');
    //    $('#OrderDatetags').tagsinput('add', { id: 'tagsPOrderFrom', label: fOrderFromDate + ' - ' + fOrderToDate });
    //    var data = $('#OrderDatetags').prev(); data.children().prepend("<b>Order Date : </b>");
    //}
    //else {
    //    $('#OrderDatetags').tagsinput('removeAll');
    //}

    //    $(".bootstrap-tagsinput").each(function () {
    //        if ($(this)[0].outerText == '') {
    //            $(this).addClass('cfilterHide');
    //        }
    //    });
    //    $('#tagscontainer').show();
    }

function SetFilterVars() {
    var queryList = '';

    var fShipmentNo = $('#txtShipmentNo').val();
    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();

    var _selVal = $("#selcustomer option:selected");
    var fcustomer = _selVal.val();
    var companyid = _selVal.data('company') ?? 0;
    if (fcustomer != '' && fcustomer!='0') queryList += "&customerid=" + fcustomer.trim();
    else queryList += "&companyid=" + companyid;

    _selVal = $("#selTransportType option:selected");
    var fTransportType = _selVal.val();
    if (fTransportType != '') queryList += "&transportType=" + fTransportType.trim();

    var fStatus = '';
    
    if (selectedDeliveryOption === "btnReadyToShip") {
        fStatus = Outboundstatus['ready to ship'];
        
        selectedDeliveryOption = '';
    } else if (selectedDeliveryOption === "btnDraftShipment") {
        fStatus = Outboundstatus['draft'];
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

    var fdeliveryIn = '';
    if (selectedDeliveryOption === "btnTodayDelivery") {
        fdeliveryIn = "day";
        selectedDeliveryOption = '';
    } else if (selectedDeliveryOption === "btn3DaysDelivery") {
        fdeliveryIn = "3days";

        selectedDeliveryOption = '';
    }
    if (fdeliveryIn !== '') queryList += "&deliveryIn=" + fdeliveryIn;


    return queryList;
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


function SetFilterVarsForPrintOutBound() {
    var queryList = '';
    var paginationInfo = getPaginationInfo();
    var fShipmentNo = $('#txtShipmentNo').val();
    queryList += "&skip=" + paginationInfo.startRecord;
    queryList += "&length=" + paginationInfo.pageSize;
    queryList += "&sortColumn=" + paginationInfo.sortingColumnName;  // Or use sortingColumnIndex if you prefer
    queryList += "&sortOrder=" + paginationInfo.sortingOrder;

    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();

    var _selVal = $("#selcustomer option:selected");
    var fcustomer = _selVal.val();
    var companyid = _selVal.data('company') ?? 0;
    if (fcustomer != '' && fcustomer != '0') queryList += "&customerid=" + fcustomer.trim();
    else queryList += "&companyid=" + companyid;
    let fcustomerName = _selVal.text();
    if (fcustomerName != '') queryList += "&customername=" + fcustomerName.trim();

    _selVal = $("#selTransportType option:selected");
    var fTransportType = _selVal.val();
    if (fTransportType != '') queryList += "&transportType=" + fTransportType.trim();

    let fTransportName = _selVal.text();
    if (fTransportName != '') queryList += "&transportName=" + fTransportName.trim();


    var fStatus = '';
    var fStatusText = '';
    var qStatusText = '';
    if (selectedDeliveryOptioncopy === "btnReadyToShip") {
        fStatus = 5;
        //fStatusText = _selVal.text();
        qStatusText = "READY TO SHIP";
        selectedDeliveryOptioncopy = '';

    } else if (selectedDeliveryOptioncopy === "btnDraftShipment") {
        fStatus = 2;
        qStatusText = "DRAFT";
        selectedDeliveryOptioncopy = '';
    } else {
        var _selVal = $("#selStatus option:selected");
        var fStatusFromDropdown = _selVal.val();
        var fStatusFromDropdownText = _selVal.text();
        if (fStatusFromDropdown !== '' || fStatusFromDropdownText!== '') {
            fStatus = fStatusFromDropdown;
            fStatusText = fStatusFromDropdownText;
            selectedDeliveryOptioncopy = '';
        }
    }
    if (fStatus !== '') queryList += "&status=" + fStatus;
    if (fStatusText !== '') queryList += "&statusText=" + fStatusText;

    var fJobNo = $('#txtJobNo').val();
    if (fJobNo != '') queryList += "&jobNo=" + fJobNo.trim();


    var fVessel = $('#txtVessel').val();
    if (fVessel != '') queryList += "&vesselName=" + fVessel.trim();


    var fromDate = $('#fromDate').val();
    if (fromDate != '' && fromDate != undefined) queryList += "&fromDate=" + fromDate;


    var toDate = $('#toDate').val();
    if (toDate != '' && toDate != undefined) queryList += "&toDate=" + toDate;

    var fdeliveryIn = '';
    if (selectedDeliveryOptioncopy === "btnTodayDelivery") {
        fdeliveryIn = "day";
        selectedDeliveryOptioncopy = '';
    } else if (selectedDeliveryOptioncopy === "btn3DaysDelivery") {
        fdeliveryIn = "3days";

        selectedDeliveryOptioncopy = '';
    }
    if (fdeliveryIn !== '') queryList += "&deliveryIn=" + fdeliveryIn;
    if (qStatusText !== '') queryList += "&quickStatus=" + qStatusText;
   
    return queryList;
}
function ViewDetails(_id) {
    $.ajax({
        url: pathname+'/Outbound/EncryptId',
        type: 'GET',
        data: { id: _id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/Outbound/EditOutboundShipmentDetails?ShipmentId=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}


function ApplyFilter() {
    var queryList = SetFilterVars();

    if (queryList.length > 0) {

        InitialiseTable(pathname + "/Outbound/GetOutboundShipmentTable?" + queryList.trim('&').trim().substring(1));
    }
    else {

        InitialiseTable(pathname + "/Outbound/GetOutboundShipmentTable");
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
            { "visible": false, "targets": 15}
        ],
        "columns": [
            {
                "data": "order_no", "name": "Shipment No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[1] > AccessLevels.NoAccess) {
                        return "<a href='#'  data-id=" + full.shipmentid + " onclick='ViewDetails(" + full.shipmentid + ")'>" + full.order_no + "</a>"

                    }
                    else {
                        return full.order_no;
                    }
                }
            },
            { "data": "shipment_statusdesc", "name": "Status", "autoWidth": true },

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
                "data": "companyCode", "name": "Party", "autoWidth": true, "render": function (data, type, full, meta) {
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
            {
                "data": "vessel_eta", "name": "Vessel ETA Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.vessel_eta != null) {
                        return moment(new Date(full.vessel_eta)).format('DD/MM/YYYY HH:mm:ss');
                    }
                    else {
                        return "";
                    }

                }
            },
            { "data": "anchorage_description", "name": "Anchorage", "autoWidth": true },
            { "data": "agent", "name": "Agent", "autoWidth": true },
            { "data": "supply_boat", "name": "Supply Boat", "autoWidth": true },
            { "data": "transport_type_description", "name": "Transport Type", "autoWidth": true },
            { "data": "driver_name", "name": "Driver", "autoWidth": true },
            { "data": "vehicle_no", "name": "Truck No", "autoWidth": true },
            { "data": "boarding_Officer_Name", "name": "Boarding Officer", "autoWidth": true },
            {
                "data": "co_party", "name": "C/O Party", "autoWidth": true, "render": function (data, type, full, meta) {
                    return data ? "Yes" : "No";
                }
            },
                

            {
                "data": "shipmentid", "name": "Actions", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.shipment_statusid > 4) {
                        return "<a href='#' style='color: #fa7d7d;pointer-events: none;' data-id=" + data + ">Delete</a>"
                    }
                    else {
                        return "<a href='#' style='color: red;' data-id=" + data + " class='delete-style';>Delete</a>"
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
    SearchData(oShipmentTable);
};


// Added by Gaurav on 10/07/2024
function PrintOutBoundList() {
    console.log("Inside the function");
    var target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }

    var _url = "";
    var queryList = SetFilterVarsForPrintOutBound();
    if (queryList.length > 0) {

        _url = (pathname + "/Outbound/PrintOutBoundList?" + queryList.trim('&').trim().substring(1));
    }
    else {
        _url = (pathname + "/Outbound/PrintOutBoundList");
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







   

   



