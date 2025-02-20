var result;
var oTable = null;
var DeliveryOrderId = null;
var shipdoctable;
$(document).ready(function () {
    $('#nav_deliveryOrders').addClass("show");
    initSettabs();
    LoadOneTimeObjs();
    $('#tbOrderinfo').click(function (e) {
        $('#tbOrderinfo').css("background-color", "#40444d");
        $('#tbShippingDetails').css("background-color", "#787373");
        $('#tbOrderLines').css("background-color", "#787373");

        $('#kt_tab_pane_1').addClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbShippingDetails').click(function (e) {
        $('#tbShippingDetails').css("background-color", "#40444d");
        $('#tbOrderinfo').css("background-color", "#787373");
        $('#tbOrderLines').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').addClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbOrderLines').click(function (e) {
        $('#tbOrderLines').css("background-color", "#40444d");
        $('#tbOrderinfo').css("background-color", "#787373");
        $('#tbShippingDetails').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').addClass("show active");
    });

    $('#btnPrintDeliveryOrder').click(function (e) {
        console.log("Inside the print del event");
        PrintDeliveryOrder();
    });

    DO_Information(pathname + "/Orders/DO_Information");
    DO_ShippingDetails(pathname + "/Orders/DO_ShippingDetails");
    InitialiseTable(pathname + "/Orders/DO_Lines");
    GetOrderAttachment(DeliveryOrderId);
});
function initSettabs() {
    $('#tbOrderinfo').css("background-color", "#40444d");
    $('#tbShippingDetails').css("background-color", "#787373");
    $('#tbOrderLines').css("background-color", "#787373");

    $('#kt_tab_pane_1').addClass("show active");
    $('#kt_tab_pane_2').removeClass("show active");
    $('#kt_tab_pane_3').removeClass("show active");
}
function LoadOneTimeObjs() {
    if (Deliveyrdstatus == null || Deliveyrdstatus == "" || Deliveyrdstatus.length == 0) {
        var OptionData = Ajax(pathname + "/Outbound/GetModuleStatuses", { "moduleId": 2 }, true);

        Deliveyrdstatus = OptionData.data.reduce((acc, item) => {
            acc[item.status_desc.toLowerCase()] = item.statusid;
            return acc;
        }, {});
    }
    
}
function DO_Information(_url) {
    $.ajax({
        type: "POST", async: false, url: _url, data: {},
        success: function (data) {
            if (data) {
                InitialiseDO_Information(data);

                //window.open(pathname + "/Orders/CustomerOrders");
                // window.location = pathname + "/Orders/CustomerOrders";
            }
        }, error: function (response) { }
    });
}
function DO_ShippingDetails(_url) {
    $.ajax({
        type: "POST", async: false, url: _url, data: {},
        success: function (data) {
            if (data) {
                InitialiseDO_ShippingDetails(data);

                //window.open(pathname + "/Orders/CustomerOrders");
                // window.location = pathname + "/Orders/CustomerOrders";
            }
        }, error: function (response) { }
    });
}
function InitialiseDO_Information(data) {
    DeliveryOrderId = data.data.delivery_order_id;
    $("#txtDeliveryOrderno").val(data.data.delivery_order_no);
    $("#txtstatus").val(data.data.status_desc);
    $("#txtshipmentNo").val(data.data.order_no);
    $("#txtSalesOrderNo").val(data.data.sales_order_no);
    $('#txtQuoteNo').val(data.data.quote_no);
    $('#txtJobNoCode').val(data.data.do_job_no);
    $('#txtCustomerCode').val(data.data.customer_no);
    $('#txtCustomerName').val(data.data.customer_name);
    $('#txtContact').val(data.data.contact);
    $('#txtVesselCode').val(data.data.vessel_code);
    $('#txtVesselName').val(data.data.vessel_name);

    var vesselETA = data.data.vessel_eta;
    if (vesselETA != null) {
        var formattedDate = new Date(vesselETA);
        var formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
            ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
            formattedDate.getFullYear() + ' ' +
            ('0' + formattedDate.getHours()).slice(-2) + ':' +
            ('0' + formattedDate.getMinutes()).slice(-2);
        $('#txtVesselETA').val(formattedDateString);
    } else { $('#txtVesselETA').val(''); }
    var postingDate = data.data.posting_date;
    if (postingDate != null) {
        formattedDate = new Date(postingDate);
        formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
            ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
            formattedDate.getFullYear() + ' ' +
            ('0' + formattedDate.getHours()).slice(-2) + ':' +
            ('0' + formattedDate.getMinutes()).slice(-2);
        $('#txtPostingDate').val(formattedDateString);
    } else { $('#txtPostingDate').val(''); }

    var reqDeliveryDate = data.data.requested_delivery_date;
    if (reqDeliveryDate != null) {
        formattedDate = new Date(reqDeliveryDate);
        formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
            ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
            formattedDate.getFullYear() + ' ' +
            ('0' + formattedDate.getHours()).slice(-2) + ':' +
            ('0' + formattedDate.getMinutes()).slice(-2);
        $('#txtReqDeliveryDate').val(formattedDateString);
    } else { $('#txtReqDeliveryDate').val(''); }
    
    var promisedDeliveryDate = data.data.promised_delivery_date;
    if (promisedDeliveryDate != null) {
        formattedDate = new Date(promisedDeliveryDate);
        formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
            ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
            formattedDate.getFullYear() + ' ' +
            ('0' + formattedDate.getHours()).slice(-2) + ':' +
            ('0' + formattedDate.getMinutes()).slice(-2);
        $('#txtPromisedDeliveryDate').val(formattedDateString);
    } else { $('#txtPromisedDeliveryDate').val(''); }

    $('#txtCustDeptCode').val(data.data.dept_code);
    $('#txtInternalDept').val(data.data.internal_dept);
    $('#txtMarkRefNo').val(data.data.mark_reference_no);
    $('#txtParentCustomer').val(data.data.parent_customer);
    $('#txtPONo').val(data.data.pono);
}

function InitialiseDO_ShippingDetails(data) {
    for (var i = 0; i < data.data.length; i++) {
        $("#txtAddressCode").val(data.data[i].addr_code);

        var shipDate = data.data[i].planned_ship_date;
        if (shipDate != null) {
            var formattedDate = new Date(shipDate);
            var formattedDateString = ('0' + formattedDate.getDate()).slice(-2) + '/' +
                ('0' + (formattedDate.getMonth() + 1)).slice(-2) + '/' +
                formattedDate.getFullYear() + ' ' +
                ('0' + formattedDate.getHours()).slice(-2) + ':' +
                ('0' + formattedDate.getMinutes()).slice(-2);
            $("#txtShipmentDate").val(formattedDateString);
        } else { $("#txtShipmentDate").val(''); }
        $("#txtAddress").val(data.data[i].address1);
        $("#txtContactPerson").val(data.data[i].contact_person);
        $("#txtAddress2").val(data.data[i].address2);
        $("#txtContactNo").val(data.data[i].contact_number);
        $("#txtAddress3").val(data.data[i].address3);
        $("#txtEmailAddress").val(data.data[i].emailid);
        $("#txtAddress4").val(data.data[i].address4);
        $("#txtLocationCode").val(data.data[i].location_code);
        $("#txtPostalCode").val(data.data[i].zipcode);
        $("#txtAgent").val(data.data[i].agent_service);
        $("#txtCity").val(data.data[i].city);
        $("#txtAgentService").val(data.data[i].agent_service);
        $("#txtCountryRegion").val(data.data[i].country_region);
    }
    

}
function InitialiseTable(_url) {
    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
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
                return data.data;
            }
        },
        "columnDefs": [
            /*{ "visible": isaccess, "targets": 7 }*/
        ],
        "columns": [
            {
                "data": "Row_No", "name": "Row No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "item_no", "name": "Item No.", "autoWidth": true },
            { "data": "item_ref_no", "name": "Item Ref No.", "autoWidth": true },
            { "data": "item_description", "name": "Description", "autoWidth": true },
            { "data": "location_code", "name": "Loc Code", "autoWidth": true },
            {
                "data": "quantity", "name": "Quantity", "autoWidth": true, "render": function (data, type, full, meta) {
                    return full.quantity.toFixed(2);
                }
            },
            { "data": "uom", "name": "UOM", "autoWidth": true },
            { "data": "internal_dept", "name": "Internal Dept Code", "autoWidth": true },
            { "data": "awb_bl", "name": "AWB/BL", "autoWidth": true },
            { "data": "jobno", "name": "Job No. Code", "autoWidth": true },
            { "data": "vessel_code", "name": "Vsl Code", "autoWidth": true },
            { "data": "sales_person_code", "name": "SalesPerson", "autoWidth": true },
            { "data": "broker_code", "name": "Broker Code", "autoWidth": true },
            { "data": "dept_code", "name": "Customer Dept Code", "autoWidth": true },
            { "data": "quantity_invoiced", "name": "Quantity Invoiced", "autoWidth": true },
            
        ],
        "initComplete": function (data) {
            $('#DOLinesTotalRecord').text(data._iRecordsTotal);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        }

    });
    // SearchData(oTable);
}

function PrintDeliveryOrder() {
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
        url: pathname + "/Orders/PrintDeliveryOrderDetails",
        data: { _deliveryOrderId: DeliveryOrderId },
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

function GetOrderAttachment(deliveryOrderId) {

    //$('#modalAttachment').modal({ backdrop: 'static', keyboard: false });
    //ClearModalFields();

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

    //$('#modalAttachment').modal('show');
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
