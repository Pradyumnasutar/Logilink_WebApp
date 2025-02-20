var oShipmentTable = null;
var oTable = null;
var oShipmentTable = null; var selectedDeliveryOption = ''; var selectedDeliveryOptioncopy = '';
var fShipmentNo, fOrderNo, fSite, fCustomer, fCurrency, fCoordinator, fStatus, fOrderFromDate, fOrderToDate, fordertype, fTransportType, fdeliveryIn, fromDate, toDate;
$(document).ready(function () {
    $('#nav_InboundShipments').addClass("show");

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
    $(document).on('click', '.shipment-view', function (e) {
        var quotationid = e.target.attributes['data-quotationid'].value;
        var inboundshipmentid = e.target.attributes['data-inboundid'].value;
        if (inboundshipmentid != null && inboundshipmentid > 0) {
            ViewinboundDetails(inboundshipmentid);
        }
        else if (quotationid != null && quotationid > 0) {
            viewquotationdetails(quotationid);
        }

    });
    $('#btnApply_Filter').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Quick Filter");
        selectedDeliveryOption = '';
        selectedDeliveryOptioncopy = '';
        ApplyFilter();
        //resetDropdownLabelColor();
    });
    $(document).on('change', '#fromDate, #toDate', function (e) {
        var isValid = validateDates();
        if (!isValid) {
            Swal.fire('Validation Error !', 'Invalid date range: From Date is greater than To Date.', 'error');
            $("#fromDate").val('').trigger('change');;
            $("#toDate").val('').trigger('change');;
        }
    })
    var myCollapsible = document.getElementById('kt_accordion_filter')
    myCollapsible.addEventListener('hidden.bs.collapse', function () {
        SetFilterVars();
        /*AddTagFilter();*/
    })
    myCollapsible.addEventListener('show.bs.collapse', function () {
        $('#tagscontainer').hide();
    })

    ApplyFilter().then(() => {
        //SetStatusDropdown();
        //SetCustomerDropdown();
        // Perform any additional actions after the filter is applied
    })
    .catch((error) => {
        console.error("Failed to apply filter:", error);
        // Handle the error gracefully
    });
    SetStatusDropdown();
    SetCustomerDropdown();
})
function clearFilters() {
    $('#txtShipmentNo').val('');
    $('#selcustomer').val('').trigger('change');
    $('#txtPONo').val('').trigger('change');
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
function ApplyFilter() {
    return new Promise((resolve, reject) => {
        try {
            var queryList = SetFilterVars();

            // Build the URL based on queryList
            var url = queryList.length > 0
                ? pathname + "/Inbound/GetInboundShipmentGrid?" + queryList.trim('&').trim().substring(1)
                : pathname + "/Inbound/GetInboundShipmentGrid";

            // Initialize the table and resolve the promise when complete
            InitialiseTable(url)
                //.then(() => {
                //    console.log("Table initialized successfully.");
                //    resolve(); // Resolve the promise
                //})
                //.catch((error) => {
                //    console.error("Error initializing table:", error);
                //    reject(error); // Reject the promise
                //});
        } catch (error) {
            console.error("Error in ApplyFilter:", error);
            reject(error); // Reject the promise for unexpected errors
        }
    });
}
function SetFilterVars() {
    var queryList = '';

    var fShipmentNo = $('#txtShipmentNo').val();
    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();

    var _selVal = $("#selcustomer option:selected");
    var fcustomer = _selVal.val();
    var companyid = _selVal.data('company') ?? 0;
    if (fcustomer != '' && fcustomer != '0') queryList += "&customerid=" + fcustomer.trim();
    else queryList += "&companyid=" + companyid;

    var fPONo = $('#txtPONo').val();
    if (fPONo != '') queryList += "&PONo=" + fPONo.trim();

    var fStatus = '';

    //if (selectedDeliveryOption === "btnReadyToShip") {
    //    fStatus = Outboundstatus['ready to ship'];

    //    selectedDeliveryOption = '';
    //} else if (selectedDeliveryOption === "btnDraftShipment") {
    //    fStatus = Outboundstatus['draft'];
    //    selectedDeliveryOption = '';
    //}
    //else {
    var _selVal = $("#selStatus option:selected");
    var fStatusFromDropdown = _selVal.val();
    if (fStatusFromDropdown !== '') {
        fStatus = fStatusFromDropdown;
        selectedDeliveryOption = '';
    }
    //}
    if (fStatus !== '') queryList += "&status=" + fStatus;


    var fJobNo = $('#txtJobNo').val();
    if (fJobNo != '') queryList += "&jobNo=" + fJobNo.trim();


    var fVessel = $('#txtVessel').val();
    if (fVessel != '') queryList += "&vesselName=" + fVessel.trim();


    var fromDate = $('#fromDate').val();
    if (fromDate != '' && fromDate != undefined) queryList += "&fromDate=" + fromDate;


    var toDate = $('#toDate').val();
    if (toDate != '' && toDate != undefined) queryList += "&toDate=" + toDate;

    //var fdeliveryIn = '';
    //if (selectedDeliveryOption === "btnTodayDelivery") {
    //    fdeliveryIn = "day";
    //    selectedDeliveryOption = '';
    //} else if (selectedDeliveryOption === "btn3DaysDelivery") {
    //    fdeliveryIn = "3days";

    //    selectedDeliveryOption = '';
    //}
    //if (fdeliveryIn !== '') queryList += "&deliveryIn=" + fdeliveryIn;


    return queryList;
}
function InitialiseTable(_url) {
    var isaccess = false;
    var isbuyer = false;
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
                try {
                    console.error("AJAX Error:", textStatus, errorThrown); // Log error details
                    // You can display a user-friendly message or handle the error further here
                } catch (error) {
                    console.error("Error in error handler:", error);
                }
            },
            "dataSrc": function (data) {
                try {
                    if (data.data != undefined) {
                        console.log("Success!");
                        totalrecords = data.recordsTotal;
                        isbuyer = data.isbuyer;
                        return data.data;
                    }
                    else {
                        console.log("No response!");
                        return [];
                    }
                    
                } catch (error) {
                    console.error("Error in dataSrc function:", error);
                    // Return an empty array if something goes wrong
                    return [];
                }
            }
        },

        "columnDefs": [
            { "visible": false, "targets": [0] }
        ],
        "columns": [
            { "data": "inboundShipmentId", "name": "", "autoWidth": true },
            {
                "data": "shipmentno", "name": "Shipment No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[13] > AccessLevels.NoAccess) {
                        return "<a href='#' class='shipment-view' data-quotationid = " + full.quotationid + " data-inboundid=" + full.inboundShipmentId + ">" + full.shipmentno + "</a>"

                    }
                    else {
                        return full.shipmentno;
                    }
                }
            },
            { "data": "status_desc", "name": "Status", "autoWidth": true },

            {
                "data": "transactionDate", "name": "Transaction Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (data != null) {
                        return moment(data).format('DD/MM/YYYY');
                    } else {
                        return "";
                    }
                }
            },
            {
                "data": "cust_name", "name": "Party", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (isbuyer) {

                        return full.companyCode + "-" + full.companyName;
                    }
                    else {
                        return full.cust_code + "-" + full.cust_name;
                    }
                }
            },
            { "data": "jobOrderNo", "name": "Job No.", "autoWidth": true },
            { "data": "internal_order_no", "name": "PO No.", "autoWidth": true },
            { "data": "vessel_name", "name": "Vessel Name", "autoWidth": true },
            { "data": "remarks", "name": "Remarks", "autoWidth": true }
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
function ViewinboundDetails(_id) {
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
function viewquotationdetails(id) {
    $.ajax({
        url: pathname + '/Inbound/EncryptId',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.open(pathname + "/Inbound/InboundShipmentDetails?quotationid=" + encodeURIComponent(encryptedId));
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}

function SetStatusDropdown() {
    var OptionData = Ajax(pathname + "/Inbound/GetModuleStatuses", { "moduleId": 2 }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';
        }
    }
    $('#selStatus')[0].innerHTML = options;
    Outboundstatus = OptionData.data.reduce((acc, item) => {
        acc[item.status_desc.toLowerCase()] = item.statusid;
        return acc;
    }, {});

}

function SetCustomerDropdown() {
    var OptionData = Ajax(pathname + "/Inbound/GetCustomers", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].customerId + ' data-company=' + OptionData.data[i].companyId + '>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';
        }
    }
    $('#selcustomer')[0].innerHTML = options;


}

