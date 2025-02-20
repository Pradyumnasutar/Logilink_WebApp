var oTable = null; selectedIdss = []; selectedSites = []; checkedItems = []; TempDoList = [];
var firstCustomerno = null; var isCheck = null;
var selCount = 0;
var selectedDeliveryOption = ''; var FilterDOCustNo = '';
var checkboxState = {}; // Dictionary to store checkbox state
var fShipmentNo, fDONo, fCustomerCode, fCustomer='', fShipmentDate, fCoordinator, fStatus, fOrderFromDate, fOrderToDate, fordertype, fTransportType, _selShipment_No, fshipmentIn ='';
$(document).ready(function () {
    var abc = 9;
    $('#nav_deliveryOrders').addClass("show");


    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"

    });
    $('#fromDate').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"

    });
    $('#dtOrderToDate').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y", minDate: "today"
    });

    InitialiseTable(pathname + "/Orders/GetDeliveryOrderList");
    var myCollapsible = document.getElementById('kt_accordion_filter')
    myCollapsible.addEventListener('hidden.bs.collapse', function () {
        //SetFilterVars();
        //AddTagFilter();
    })
    myCollapsible.addEventListener('show.bs.collapse', function () {
        $('#tagscontainer').hide();
    })
    $('#btnApply_Filter').click(function () {
        ApplyFilter();
    })
    SetCustomerDropdown();
    //LoadCustomers();
    $(document).on('change', '#dtdatefrom, #dtdateto', function (e) {
        var isValid = validateDates();
        if (!isValid) {
            Swal.fire('Validation Error !', 'Invalid date range: From Date is greater than To Date.', 'error');
            $("#dtdatefrom").val('').trigger('change');;
            $("#dtdateto").val('').trigger('change');;
        }
    })
    
    $('#btnAsssigntoship').click(function (e) {
        View_Location(e);
    });
    $('#btnAssignConform').click(function () {
        AssignToShipment();
    });
    $('#selectall').click(function (e) {
        e.stopPropagation();
        eventCheckBox();
    });

    // 'Check All' functionality
    $('#CheckAll').on('change', function (e) {

        e.stopPropagation();
        var isChecked = $(this).is(':checked');
        $('.recordCheckbox').prop('checked', isChecked).each(function () {
            var id = $(this).attr('data-delivery_order_id');
            //var id = $(this).attr('data-id');
            checkboxState[id] = isChecked;
        });
        if (isChecked) {
            // Filter TempDoList for items where status_desc is "OPEN"
            var filteredItems = TempDoList.filter(function (item) {
                return item.status_desc.toUpperCase() === "OPEN";
            });

            // Extract delivery_order_id values from filteredItems
            checkedItems = filteredItems.map(function (item) {
                return item.delivery_order_id;
            });


        }
        else {
            
            $('.recordCheckbox').each(function () {
                var id = $(this).attr('data-delivery_order_id');
                //var id = $(this).attr('data-id');
                $(this).prop('checked', false); //checkboxState[id] ||
            });
            checkedItems = {};
           
        }

    });

    // Store checkbox state before each draw
    $('#tblData').on('preDraw.dt', function (e) {
        e.stopPropagation();
        var fghj = 0;
        $('.recordCheckbox').each(function () {
            var id = $(this).attr('data-delivery_order_id');
            //var id = $(this).attr('data-id');
            checkboxState[id] = $(this).prop('checked');
        });
        var ghk = $('#CheckAll').prop('checked');
        if (!ghk) {
            //$('.recordCheckbox').prop('checked', false);
            $('.recordCheckbox').each(function () {
                var id = $(this).attr('data-delivery_order_id');
                //var id = $(this).attr('data-id');
                $(this).prop('checked',false);
            });
        }
        var fghj = $('.recordCheckbox:checked').length;
    });

    // Restore checkbox state after each draw
    $('#tblData').on('draw.dt', function (e) {
        e.stopPropagation();
        $('.recordCheckbox').each(function () {
            var id = $(this).attr('data-delivery_order_id');
            //var id = $(this).attr('data-id');
            $(this).prop('checked', false); //checkboxState[id] ||
        });
        var isChecked = $('#CheckAll').is(':checked');
        // Also restore the 'Check All' checkbox state
        var allChecked = $('.recordCheckbox').length == $('.recordCheckbox:checked').length;
        if ($('.recordCheckbox').length == 0 && $('.recordCheckbox:checked').length == 0) {
            allChecked = false;

            $('#CheckAll').prop('checked', false);
            $('.recordCheckbox').prop('checked', false);
        }
        if ($('.recordCheckbox').length > 0 && isChecked) { $('#CheckAll').prop('checked', true); $('.recordCheckbox').prop('checked', true); }
        else if ($('.recordCheckbox:checked').length > 0) {
            //$('.recordCheckbox').prop('checked', true);
            var ghj = $(this).prop('checked');
            //$('.recordCheckbox').each(function () {
            //    //var deliveryOrderId = $(this).data('delivery_order_id');
            //    var deliveryOrderId = $(this).attr('data-delivery_order_id');
            //    if (checkedItems.includes(deliveryOrderId)) {
            //        $(this).prop('checked', true);
            //    } else {
            //        $(this).prop('checked', false);
            //    }
            //});

            $('.recordCheckbox').each(function () {
                var deliveryOrderId = $(this).attr('data-delivery_order_id');
                if (Array.isArray(checkedItems) && checkedItems.indexOf(deliveryOrderId) !== -1) {
                    $(this).prop('checked', true);
                } else {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $('.recordCheckbox').prop('checked', false);
        }

    });

    // Individual checkbox change
    $(document).on('change', '.recordCheckbox', function (e) {
        e.stopPropagation();
        var id = $(this).attr('data-delivery_order_id');
        //var id = $(this).attr('data-id');
        checkboxState[id] = $(this).prop('checked');

        // Update 'Check All' checkbox state
        var allChecked = $('.recordCheckbox').length === $('.recordCheckbox:checked').length;
        $('#CheckAll').prop('checked', allChecked);
    });


    $(document).on('click', '.btnDownloadDO', function () {
        var deliveryDocumentId = $(this).attr('data-docid');
        var DOId = $(this).attr('data-DOid');
        DownloadAttachment(deliveryDocumentId, DOId)
    });

    $(document).on('click', '.recordCheckbox', function (e) {
        e.stopPropagation();
        if (firstCustomerno !== null && selCount > 0) {
            var scndCustNo = $(this).attr('data-delivery_customerno');
            var isbool = $(this).prop('checked');
            //var dId = $(this).data('delivery_order_id');
            var dId = $(this).attr('data-delivery_order_id');
            if (firstCustomerno === scndCustNo) {
                if (isbool) {
                    selCount++;
                    FilterDOCustNo = scndCustNo;
                    checkedItems.push(dId);
                }
                else {
                    selCount--;
                    let index = checkedItems.indexOf(dId);
                    if (index !== -1) {
                        checkedItems.splice(index, 1);  // Remove dId from checkedItems array
                    }
                }

            }
            else {
                $(this).prop('checked', false);
                Swal.fire('', 'Please select same customer.', 'warning');
            }
        }
        else {
            firstCustomerno = $(this).attr('data-delivery_customerno');
            selCount++;
            //var dId = $(this).data('delivery_order_id');
            var dId = $(this).attr('data-delivery_order_id');
            checkedItems.push(dId);
            FilterDOCustNo = firstCustomerno;
        }

    });

    $('#btnCreateShipment').click(function () {
        ValidateDOsforNewShipment(pathname + '/Outbound/ValidateDOswithNewShipment');

    });

    SetStatusDropdown();

    $('#btnPrintDeliveryOrder').click(function () {
        PrintDeliveryOrderList();
    });

    $('#btnUnassignedDO').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Unassigned DO").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnUnassignedDO';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    $('#btnShipmentIn14Days').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Shipment In 14 Days").removeClass('btn-secondary').addClass('btn-primary');
        selectedDeliveryOption = 'btnShipmentIn14Days';
        ApplyFilter();
        clearFilters();
        $('#dropdownMenuButton').dropdown('toggle');

    });

    function resetDropdownLabelColor() {
        $('#dropdownMenuButton').text("Quick Filter").removeClass('btn-primary').addClass('btn-secondary');
    }

    $('#btnApply_Filter').click(function (e) {
        e.stopPropagation();
        $('#dropdownMenuButton').text("Quick Filter");
        selectedDeliveryOption = '';
        ApplyFilter();
        resetDropdownLabelColor();
    });

});
//function LoadCustomers() {
//    $('#selcustomer').select2({
//        placeholder: 'Select an option',
//        ajax: {
//            url: pathname + "/Outbound/GetCustomers",  // Replace with your actual endpoint
//            dataType: 'json',
//            delay: 250,  // Delay in milliseconds (useful to reduce the number of requests)
//            data: function (params) {
//                return {
//                    q: params.term  // Search query based on the user input
//                };
//            },
//            processResults: function (data) {

//                return {
//                    results: data.data.map(item => ({
//                        id: item.customerId,
//                        text: item.cust_Code + '-' + item.cust_Name
//                    }))
//                };
//            },
//            cache: true  // Cache the results to avoid making repeated requests
//        },
//        language: {
//            inputTooShort: function () {
//                return "Please search and select customer!";  // Change this message
//            }
//        },
//        minimumInputLength: 1  // Minimum characters required to trigger the AJAX call
//    });
//}

//function LoadCustomers() {
//    $.ajax({
//        url: pathname + "/Outbound/GetCustomers",  // Replace with your actual endpoint
//        dataType: 'json',
//        success: function (data) {
//            // Initialize select2 after fetching all customer data
//            $('#selcustomer').select2({
//                placeholder: 'Select an option',
//                data: data.data.map(item => ({
//                    id: item.customerId,
//                    text: item.cust_Code + '-' + item.cust_Name
//                })),
//                cache: true,
//                minimumInputLength: 0  // No minimum input length since data is already loaded
//            });
//        },
//        error: function () {
//            alert('Failed to load customer data.');
//        }
//    });
//}

function ValidateDOsforNewShipment(_url) {
    var fghj = 0;
    $.ajax({
        type: "POST", async: false,
        url: _url,
        data: { _DOids: checkedItems },
        success: function (response) {
            try {
                if (response.data.isSuccess == true) {
                    //var msg = "Delivery orders validate successfully for outbound shipment";
                    var msg = response.data.message;
                    Swal.fire({
                        text: msg,
                        icon: "success",
                        buttonsStyling: !1,
                        showCancelButton: true,
                        cancelButtonText: 'Close',
                        confirmButtonText: "Go To Create Shipment",
                        customClass: { cancelButton: "btn btn-secondary", confirmButton: "btn btn-success" },
                        showClass: {
                            popup: 'animate__animated animate__fadeInDown'
                        },
                        hideClass: {
                            popup: 'animate__animated animate__fadeOutUp'
                        }
                    }).then((function (e) {
                        if (e.isConfirmed) {

                            let baseUrl = pathname + '/Outbound/CreateOutboundShipmentDetails';
                            var dataToSend = {
                                _model: JSON.stringify(response.data)

                            };
                            redirectWithData(baseUrl, dataToSend);
                            //window.location = pathname + '/Outbound/CreateOutboundShipmentDetails';
                        }

                    }));
                }
                else {
                    if (response.data.message != '') {
                        Swal.fire('Validation Error !', response.data.message, 'error');
                    }
                    else {
                        Swal.fire('Validation Error !', 'Something Went Wrong.', 'error');
                    }
                }
            }
            catch (err) {
                Swal.fire('Error', 'Unable to Assign Delivery Orders', 'error');
            }
        },
        error: function (response) {
            if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                Swal.fire('Unable to Assign Delivery Orders', "Please enter unique code", 'error');
            }
            else Swal.fire('Unable to Assign Delivery Orders', response.responseText, 'error');
        }
    });

}
function redirectWithData(url, data) {
    // Create a form element
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = url;

    // Append hidden input fields
    for (var key in data) {
        if (data.hasOwnProperty(key)) {
            var hiddenField = document.createElement('input');
            hiddenField.type = 'hidden';
            hiddenField.name = key;
            hiddenField.value = data[key];

            form.appendChild(hiddenField);
        }
    }

    // Append the form to the body and submit it
    document.body.appendChild(form);
    form.submit();
}
function eventCheckBox(selectAllCheckbox) {
    oTable.page.len(-1).draw();
    var checkboxes = document.querySelectorAll('.recordCheckbox');
    // Iterate through each checkbox and set its checked property
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = selectAllCheckbox.checked;
    });

}
function SetStatusDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetModuleStatuses", { "moduleId": 2 }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';
        }
    }
    $('#selstatus')[0].innerHTML = options;

    Deliveyrdstatus = OptionData.data.reduce((acc, item) => {
        acc[item.status_desc.toLowerCase()] = item.statusid;
        return acc;
    }, {});


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

function SetShipmentListDropdown() {
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var OptionData = Ajax(pathname + "/Outbound/SetShipmentListDropDown", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            if (OptionData.data[i].cust_code == FilterDOCustNo) {
                //OptionData.data[i].shipment_statusid == Deliveyrdstatus['open'] && 
                options += '<option value=' + OptionData.data[i].shipmentid + '>' + OptionData.data[i].order_no + '</option>';
            }
        }
    }
    $('#toShipmentList')[0].innerHTML = options;
}
function SetCustomerDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetCustomers", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            //options += '<option value=' + OptionData.data[i].customerId + '>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';
            options += '<option value=' + OptionData.data[i].customerId + ' data-company=' + OptionData.data[i].companyId + '>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';

        }
    }
    $('#selcustomer')[0].innerHTML = options;


}
function AssignToShipment() {
    var _selOpt = $("#toShipmentList option:selected");
    var _Shipval = _selOpt[0].value;
    _selShipment_No = _selOpt[0].innerText;
    //var _DorderNos = selectedSites;
    var _DorderNos = checkedItems;
    if (_Shipval != '' && _DorderNos != '') {
        $.ajax({
            type: "POST", async: false,
            url: pathname + "/Orders/AssignToShipment",
            data: { "id": _Shipval, "deliveryOrderNos": _DorderNos },
            success: function (response) {
                try {
                    if (response.result == true) {
                        var msg = "Delivery orders assigned successfully to outbound shipment '" + _selShipment_No + "'!";

                        Swal.fire({
                            text: msg,
                            icon: "success",
                            buttonsStyling: !1,
                            showCancelButton: true,
                            cancelButtonText: 'Close',
                            confirmButtonText: "Go To Shipment Order",
                            customClass: { cancelButton: "btn btn-secondary", confirmButton: "btn btn-success" },
                            showClass: {
                                popup: 'animate__animated animate__fadeInDown'
                            },
                            hideClass: {
                                popup: 'animate__animated animate__fadeOutUp'
                            }
                        }).then((function (e) {
                            if (e.isConfirmed) {
                                ViewShipmentDetails(_Shipval);
                            }

                        }));
                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Validation Error !', response.msg, 'error');
                        }
                        else {
                            Swal.fire('Validation Error !', 'Something Went Wrong.', 'error');
                        }
                    }
                }
                catch (err) {
                    Swal.fire('Error', 'Unable to Assign Delivery Orders', 'error');
                }
            },
            error: function (response) {
                if (response.responseText.indexOf("Violation of UNIQUE KEY") > 0) {
                    Swal.fire('Unable to Assign Delivery Orders', "Please enter unique code", 'error');
                }
                else Swal.fire('Unable to Assign Delivery Orders', response.responseText, 'error');
            }
        });
    }
    else Swal.fire('Error !', 'You cannot assign a Delivery Order to a Released Shipment Order', 'warning');

}
function ViewShipmentDetails(_id) {
    $.ajax({
        url: pathname + '/Orders/EncryptId',
        type: 'GET',
        data: { id: _id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.location = pathname + "/Outbound/EditOutboundShipmentDetails?ShipmentId=" + encodeURIComponent(encryptedId);
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
    //window.location = pathname + "/Outbound/EditOutboundShipmentDetails?ShipmentId=" + _id;
}


function View_Location(e) {

    var selectedCheckboxes = $('input.recordCheckbox:checked');
    //var firstCustomerno = selectedCheckboxes.first().attr('data-delivery_customerno');
    selectedCheckboxes.each(function () {
        var statusid = $(this).attr('data-delivery_statusid'); // Get the status of the current checkbox
        //var customerno = $(this).attr('data-delivery_customerno');
        var _id = Deliveyrdstatus['open'].toString();
        // Check if status is "OPEN"
        if (statusid !== _id) {
            $(this).prop('checked', false); // Unselect the checkbox
        }
        //if (customerno !== firstCustomerno) {
        //    $(this).prop('checked', false); // Unselect the checkbox
        //}
    });
    selectedCheckboxes = selectedCheckboxes.filter(function () {
        return $(this).prop('checked'); // Keep only checkboxes where checked attribute is true
    });
    selectedIdss = selectedCheckboxes.map(function () {
        return $(this).attr('data-delivery_order_no');
    }).get();

    selectedSites = selectedCheckboxes.map(function () {
        return $(this).attr('data-delivery_order_id');
    }).get();
    var unique = selectedSites.filter(function (itm, i, a) {
        return i == a.indexOf(itm);
    });

    var checkbox = document.getElementById('CheckAll');

    checkbox.addEventListener('change', function () {
        isCheck = checkbox.checked;
        // Your logic for handling isChecked goes here
    });

    if (checkedItems.length > 0 && checkbox.checked) { }
    else if (checkedItems.length > 0) { }
    else {
        checkedItems = selectedSites;
    }
    $("#selDOrder").text(" " + checkedItems.length + " ");

    // Loop through each checkbox
    if (checkedItems.length > 0) {
        $('#modallocation').modal({ backdrop: 'static', keyboard: false });
        ClearModalFields();
        $('#modalItems select').select2({
            dropdownParent: "#modalItems",
            dropdownAutoWidth: true
        });
        SetShipmentListDropdown();

        $('#modallocation').modal('show');
    } else {
        Swal.fire('', 'Please check the selected Delivery Orders.', 'warning');
    }

}

function ClearModalFields() {
    $('#modallocation input').val('');
    $('#modallocation textarea').val('');
    $('#modallocation select').val('').trigger('change');
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

function SetFilterVars() {
    var queryList = '';

    fDONo = $('#txtDONo').val();
    if (fDONo != '') queryList += "&dono=" + fDONo.trim();
    var paginationInfo = getPaginationInfo();

    queryList += "&skip=" + paginationInfo.startRecord;
    queryList += "&length=" + paginationInfo.pageSize;
    queryList += "&sortColumn=" + paginationInfo.sortingColumnName;  // Or use sortingColumnIndex if you prefer
    queryList += "&sortOrder=" + paginationInfo.sortingOrder;
    //fCustomerCode = $('#txtCustomerCode').val();
    //if (fCustomerCode != '') queryList += "&customerCode=" + fCustomerCode.trim();
    var fcustval = $("#selcustomer").val();
    var fcustomer = $("#selcustomer option:selected");
    var companyid = fcustomer.data('company') ?? 0;
    if (fcustval != '' && fcustval != '0') queryList += "&customerid=" + fcustval.trim();
    else queryList += "&companyid=" + companyid;


    var _selVal = $("#selstatus option:selected");
    var fStatus = '';
    var fStatusdesc = '';
    if (selectedDeliveryOption === "btnUnassignedDO") {
        fStatus = Deliveyrdstatus['open']
        fStatusdesc = 'Open';
        //fStatus = 5;
        //selectedDeliveryOption = '';
        queryList += "&btnUnassignedDO=" + 1;
    }
    else {
        var _selVal = $("#selstatus option:selected");
        var fStatusFromDropdown = _selVal.val();
        fStatusdesc = _selVal.text();
        if (fStatusFromDropdown !== '') {
            fStatus = fStatusFromDropdown;
            //selectedDeliveryOption = '';
        }
    }
    if (fStatus !== '') queryList += "&status=" + fStatus;
    if (fStatusdesc !== '') queryList += "&statusdesc=" + fStatusdesc;

    fShipmentDate = $("#dtshipment").val();
    if (fShipmentDate != '') queryList += "&shipmentDate=" + fShipmentDate.trim();
    fOrderFromDate = $("#dtdatefrom").val();
    if (fOrderFromDate != '') queryList += "&orderFromDate=" + fOrderFromDate.trim();
    fOrderToDate = $("#dtdateto").val();
    if (fOrderToDate != '') queryList += "&orderToDate=" + fOrderToDate.trim();
    fShipmentNo = $('#txtShipmentNo').val();
    if (fShipmentNo != '') queryList += "&shipmentNo=" + fShipmentNo.trim();

    var fshipmentIn = '';
    if (selectedDeliveryOption === "btnShipmentIn14Days") {
        fshipmentIn = "14days";

        //selectedDeliveryOption = '';
    }
    if (fshipmentIn !== '') queryList += "&shipmentIn=" + fshipmentIn;


    return queryList;
}


function validateDates() {
    var fromDate = $("#dtdatefrom").val();
    var toDate = $("#dtdateto").val();
    
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
    var queryList = SetFilterVars();

    if (queryList.length > 0) {
        InitialiseTable(pathname + "/Orders/GetDeliveryOrderList?" + queryList.trim('&').trim().substring(1));
    }
    else {
        InitialiseTable(pathname + "/Orders/GetDeliveryOrderList");
    }
}
var SearchData = (oTable) => {
    const filterSearch = document.querySelector('[data-kt-filter="search"]');
    filterSearch.addEventListener('keyup', function (e) {
        oTable.search(e.target.value).draw();
    });
}
function InitialiseTable(_url) {
    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
    if (slModuleAction[1002] > 3) {
        isaccess = true;
    }
    var rdVal = Math.floor(Math.random() * 100) + 1;
    oTable = $("#tblData").DataTable({
        "language": {
            "infoFiltered": ""
        },
        //"scrollY": '40vh',
        //"scrollCollapse": false,
        //"scrollX": true,
        "processing": true,
        "serverSide": true,
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
            "url": _url,
            "data": { ValueRD: rdVal },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {

            },

            "dataSrc": function (data) {
                TempDoList = data.tempDoList;
                return data.data;
            }
        },
        "columnDefs": [

            { "orderable": false, "targets": [0] }
        ],
        "columns": [
            {
                "data": "", "name": "", "autoWidth": true, "render": function (data, type, full, meta) {
                    //return ' <td style="width:5%"><input  type="checkbox" class="form-check-input h-15px w-15px recordCheckbox" data-delivery_order_id="' + full.delivery_order_id + '" data-delivery_order_no="' + full.delivery_order_no + '" data-delivery_statusid="' + full.statusid + '" data-delivery_customerid="' + full.customerid + '"></td>';
                    if (full.statusid === 1) {
                        return '<td style="width:5%"><input type="checkbox" class="form-check-input h-15px w-15px recordCheckbox" style="border:1px solid rgba(0,0,0,.50)" id="selCheckbox" checked="false" data-delivery_order_id="' + full.delivery_order_id + '" data-delivery_order_no="' + full.delivery_order_no + '" data-delivery_statusid="' + full.statusid + '" data-delivery_customerno="' + full.customer_no + '"></td>';

                    }
                    else {
                        return '';

                    }
                }
            },

            {
                "data": "delivery_order_no", "name": "DO No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (slModuleAction[2] > AccessLevels.NoAccess) {
                        return "<a href='#' data-id=" + full.delivery_order_id + " onclick='ViewDetails(" + full.delivery_order_id + ")'>" + full.delivery_order_no + " </a> "
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
            {
                "data": "shipmentdate", "name": "Shipment Date", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.shipmentdate != null) {
                        return moment(new Date(full.shipmentdate)).format('DD/MM/YYYY');
                    }
                    else {
                        return "";
                    }
                }
            },
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
            { "data": "order_no", "name": "Shipment No.", "autoWidth": true },
            { "data": "sales_person_code", "name": "Sales Person Code", "autoWidth": true },
            {
                "data": "", "name": "Attachment", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<a href="#" data-id="' + full.delivery_order_id + '" onclick="GetOrderAttachment(' + full.delivery_order_id + ')"><i class="fa fa-paperclip fs-3 me-2" aria-hidden="true"></i></a>';
                }
            },

        ],
        "initComplete": function (data) {
            $('#lblTotalRecord').text(data._iRecordsTotal);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
            //$('.recordCheckbox').each(function () {
            //    //var deliveryOrderId = $(this).data('delivery_order_id');
            //    var deliveryOrderId = $(this).attr('data-delivery_order_id');
            //    if (checkedItems.includes(deliveryOrderId)) {
            //        $(this).prop('checked', true);
            //    } else {
            //        $(this).prop('checked', false);
            //    }
            //});

            $('.recordCheckbox').each(function () {
                var deliveryOrderId = $(this).attr('data-delivery_order_id');
                if (Array.isArray(checkedItems) && checkedItems.indexOf(deliveryOrderId) !== -1) {
                    $(this).prop('checked', true);
                } else {
                    $(this).prop('checked', false);
                }
            });

        }

    });

    SearchData(oTable);
}
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


function DeleteRow() {
    alert('Deleting Row');
}

function PrintDeliveryOrderList() {
    var target = document.querySelector("#baseCard"); var blockUI = new KTBlockUI(target, {
        overlayClass: " bg-white bg-opacity-10"
    });
    blockUI.block();
    var overlay = document.querySelector('.blockui-overlay');
    if (overlay) {
        overlay.style.position = 'fixed';
    }

    var _url = "";
    var queryList = SetFilterVars();
    if (queryList.length > 0) {

        _url = (pathname + "/Orders/PrintDeliveryOrderList?" + queryList.trim('&').trim().substring(1));
    }
    else {
        _url = (pathname + "/Orders/PrintDeliveryOrderList");
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

function clearFilters() {
    $('#txtDONo').val('');
    $('#txtShipmentNo').val('');
    $('#txtCustomer').val('');
    $('#selstatus').val('').trigger('change');
    $('#dtshipment').val('');
    $('#dtdatefrom').val('');
    $('#dtdateto').val('');
    // resetDropdownLabelColor();
}