var oDoTable = null;
var shipattachdoctable = null;
var IsDoPresent = false;
$(document).ready(function () {
    
    $('#nav_OutboundShipments').addClass("show");
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"

    });
    $('.datetimeField').flatpickr({
        enableTime: true,
        time_24hr: true,
        dateFormat: "d/m/Y H:i",
        static: "true"

    });
    $('.timeField').flatpickr({

        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true


    });
    SetTransportTypeDropdown();
    SetCustomerDropdown();

    SetStatusDropdown();
    SetAnchorageDropdown();
    initSettabs();
    SetVesselDropdown();
    SetValuesFromDOs();
    $('#tbshipmentinfo').click(function (e) {
        $('#tbshipmentinfo').css("background-color", "#40444d");
        $('#tbtripplan').css("background-color", "#787373");
        $('#tbdeliveryorder').css("background-color", "#787373");

        $('#kt_tab_pane_1').addClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbtripplan').click(function (e) {
        $('#tbtripplan').css("background-color", "#40444d");
        $('#tbshipmentinfo').css("background-color", "#787373");
        $('#tbdeliveryorder').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').addClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbdeliveryorder').click(function (e) {
        $('#tbdeliveryorder').css("background-color", "#40444d");
        $('#tbshipmentinfo').css("background-color", "#787373");
        $('#tbtripplan').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').addClass("show active");
    });
    $('#btnSaveInboundShipment').click(function (e) {
        saveOutboundShipment();
    });
    $('#selstatus').siblings('.select2').children('.selection').children('.select2-selection').css({
        'background-color': '#eff2f5',
        'opacity': '1'
    });
    $(document).on('click', '.do-download', function (e) {
        var _id = e.target.attributes['data-id'].value;
        GetOrderAttachment(_id);
    });
    $(document).on('click', '.downloadDO', function (e) {
        var _id = e.target.attributes['data-docid'].value;
        DownloadAttachment(_id);
    });
    $(document).on('click', '.ViewDODetails', function (e) {
        var _id = e.target.attributes['data-id'].value;
        ViewDODetails(_id);
    });
});
function initSettabs() {
    $('#tbshipmentinfo').css("background-color", "#40444d");
    $('#tbtripplan').css("background-color", "#787373");
    $('#tbdeliveryorder').css("background-color", "#787373");

    $('#kt_tab_pane_1').addClass("show active");
    $('#kt_tab_pane_2').removeClass("show active");
    $('#kt_tab_pane_3').removeClass("show active");
}
function SetValuesFromDOs() {

    var OptionData = Ajax(pathname + "/Outbound/InitializeCreateShipmentWithDOs", {}, true);
    if (OptionData != undefined && OptionData.data.data != null && OptionData.data.data.deliveryorders.length > 0) {
        $('#selcustomer').val(OptionData.data.data.customerid);
        $('#selvessel').val(OptionData.data.data.vesselid);
        if (OptionData.data.data.vesselid !== null) {
            $('#selvessel').prop('disabled', true);
        }
        $('#selcustomer').siblings('.select2').children('.selection').children('.select2-selection').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });
        $('#selvessel').siblings('.select2').children('.selection').children('.select2-selection').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });
        $('#txtjobno').val(OptionData.data.data.jobno);
        if (OptionData.data.data.jobno !== null) {
            $('#txtjobno').prop('disabled', true);
        }
        $('#txtvesseletadate').val(moment(new Date(OptionData.data.data.deliveryorders[0].vessel_eta)).format('DD/MM/YYYY HH:mm'));
        
        if (OptionData.data.data.deliveryorders[0].vessel_eta !== null) {
            $('#txtvesseletadate').prop('disabled', true);
            $('#txtvesseletadate').css({
                'background-color': '#eff2f5',
                'opacity': '1'
            });
        }
        Model.deliveryOrderIds = [];
        if (OptionData.data.data.deliveryorders.length > 0) {
            IsDoPresent = true;
        }
        for (var i = 0; i < OptionData.data.data.deliveryorders.length; i++) {
            Model.deliveryOrderIds.push(OptionData.data.data.deliveryorders[i].delivery_order_id)
        }
        // Initialize DataTable with retrieved data directly
        oDoTable = $("#tblDeliveryorders").DataTable({
            "language": {
                "infoFiltered": ""
            },
            "processing": true,
            "serverSide": false, // Since data is already retrieved, set serverSide to false
            "data": OptionData.data.data.deliveryorders, // Use the retrieved data directly
            "order": [[1, "desc"]],
            "lengthMenu": [
                [10, 25, 50, -1],
                ['10 rows', '25 rows', '50 rows', 'Show all']
            ],
            "pageLength": 10,
            "bDestroy": true,
            "columnDefs": [

            ],
            "columns": [
                {
                    "data": "delivery_order_no", "name": "Delivery Order No", "autoWidth": true,
                    "render": function (data, type, full, meta) {
                        return "<a href='#' class='ViewDODetails' data-id='" + full.delivery_order_id + "'>" + full.delivery_order_no + "</a>";
                    }
                },
                { "data": "order_no", "name": "Order No", "autoWidth": true },
                { "data": "internal_dept", "name": "Internal Dept Code", "autoWidth": true },
                { "data": "dept_code", "name": "Customer Dept.", "autoWidth": true },
                { "data": "status_desc", "name": "DO status", "autoWidth": true },
                { "data": "sales_person_code", "name": "Sales Person Code", "autoWidth": true },
                {
                    "data": null, "name": "DO Attachment", "autoWidth": true,
                    "render": function (data, type, full, meta) {
                        return '<a href="#"><i class="fa fa-paperclip fs-3 me-2 do-download" data-id="' + full.delivery_order_id + '" aria-hidden="true"></i></a>';
                    }
                },
            ],
            "initComplete": function (data) {
                if (data.aoData.length > 0) {
                    $('#selcustomer').prop('disabled', true);

                } else {
                    $('#selcustomer').prop('disabled', false);
                }
                $('#deliveryordersCount').text(data.aoData.length);
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            },
            "drawCallback": function (settings) {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }
        });

    }
}
function SetStatusDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetModuleStatuses", { "moduleId": 1 }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {

        Outboundstatus = OptionData.data.reduce((acc, item) => {
            acc[item.status_desc.toLowerCase()] = item.statusid;
            return acc;
        }, {});

        for (var i = 0; i < OptionData.data.length; i++) {
            if (OptionData.data[i].statusid == Outboundstatus['new']) {
                options += '<option selected value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';

            }
            else {
                options += '<option value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';

            }
        }

    }

    $('#selstatus')[0].innerHTML = options;



}
function SetVesselDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetVesselMaster", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {

            options += '<option value=' + OptionData.data[i].vesselId + '>' + OptionData.data[i].vesselName + '</option>';


        }
    }
    $('#selvessel')[0].innerHTML = options;
}
function SetCustomerDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetCustomers", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].customerId + ' data-company=' + OptionData.data[i].companyId + '>' + OptionData.data[i].cust_Code + " - " + OptionData.data[i].cust_Name + '</option>';

        }
    }
    $('#selcustomer')[0].innerHTML = options;


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
function SetTransportTypeDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetTransportTypeList", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].transport_type_id + '>' + OptionData.data[i].transport_type_description + '</option>';
        }
    }
    $('#seltransporttype')[0].innerHTML = options;

}
function SetAnchorageDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetAnchorageMast", {}, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].anchorageId + '>' + OptionData.data[i].anchorageCode + " - " + OptionData.data[i].anchorageDescription + '</option>';
        }
    }
    $('#selanchorage')[0].innerHTML = options;
}
function GetCreatedShipmenInfo() {
    var ShipmentId = 0;
    var statusid = $('#selstatus').val();
    var Orderno = $('#txtshipmentno').val();
    var Jobno = $('#txtjobno').val();
    var customerid = getValidcustId();
    var transporttypeid = $('#seltransporttype').val();
    var vesselid = $('#selvessel').val();
    //var vesselcode = $('#txtvesselcode').val();
    //var vesselname = $('#txtvesselname').val();
    var vesselata = $('#txtvesselatadate').val();
    var vesseleta = $('#txtvesseletadate').val();
    var anchorageid = $('#selanchorage').val();
    var agent = $('#txtagent').val();
    var agentcontactperson = $('#txtagentcontactperson').val();
    var agentcontactno = $('#txtagentcontact').val();
    var loadingpoint = $('#txtloadingpoint').val();
    var supplyboat = $('#txtsupplyboat').val();
    var boatcontactperson = $('#txtboatcontactperson').val();
    var boatcontactno = $('#txtboatcontactno').val();
    var CoParty = $('#txtcoparty').val();
    var shipmentnotes = $('#txtRemarks').val();
    var deliverydate = $('#dtdeliverydate').val();
    var loadingtime = $('#dtloadingtime').val();

    if (vesseleta != null && vesseleta != '') {
        
        Model.shipment_Info.vessel_eta = vesseleta;
    }
    else {
        Model.shipment_Info.vessel_eta = null;
    }


    if (vesselata != null && vesselata != '') {
       
        Model.shipment_Info.vessel_ata = vesselata;
    }
    else {
        Model.shipment_Info.vessel_ata = null;
    }


    if (deliverydate != null && deliverydate != '') {
       
        Model.shipment_Info.delivery_date = deliverydate;
    }
    else {
        Model.shipment_Info.delivery_date = null;
    }


    if (loadingtime != null && loadingtime != '') {
       
        Model.shipment_Info.loading_time = loadingtime;
    }
    else {
        Model.shipment_Info.loading_time = null;
    }



    Model.shipment_Info.shipment_statusid = statusid;
    Model.shipment_Info.order_no = Orderno;
    Model.shipment_Info.jobno = Jobno;
    Model.shipment_Info.receiverid = customerid;
    Model.shipment_Info.transport_type_id = transporttypeid;
    Model.shipment_Info.vessel_id = vesselid
    //Model.shipment_Info.vessel_code = vesselcode;
    //Model.shipment_Info.vessel_name = vesselname;
    Model.shipment_Info.anchorage_id = anchorageid;
    Model.shipment_Info.agent = agent;
    Model.shipment_Info.agent_contact_person = agentcontactperson;
    Model.shipment_Info.loading_point = loadingpoint;
    Model.shipment_Info.supply_boat = supplyboat;
    Model.shipment_Info.supply_boat_contact_person = boatcontactperson;
    Model.shipment_Info.supply_boat_contact_no = boatcontactno;
    Model.shipment_Info.co_party = CoParty;
    Model.shipment_Info.shipment_notes = shipmentnotes;
    Model.shipment_Info.shipmentid = ShipmentId;
    Model.shipment_Info.agent_contact_no = agentcontactno;

}
function GetCreatedtripPlan() {
    var ShipmentId = 0;
    var BoaringOfficerName = $('#txtboardingofficer').val();
    var driverallowance = $('#txtdriverallowance').val();
    var drivercode = $('#txtdrivercode').val();
    var transportCompany = $('#txttransportcompany').val();
    var drivername = $('#txtdrivername').val();
    var OutsourcedVehicleNo = $('#txtxoutsourcedvehicleno').val();
    var OutsourcedContactPerson = $('#txtOutsouecedcontactperson').val();
    var OutsourcedContactNo = $('#txtoutsourcedcontactno').val();
    var EstimatePackingUnit = $('#txtpackingunit').val();
    var CTM = $('#txctm').val();
    var Locationfrom = $('#txtfromloc').val();
    var Locationto = $('#txttoloc').val();
    var PlannedFrom = $('#dtplannedstartdate').val();
    var PlannedTo = $('#dtplannedenddate').val();
    var DriverContact = $('#txtDriercontact').val();
    var vehicalNo = $('#txtVehicalno').val();
    var ActualDOstart = $('#dtactualdeliverystart').val();
    var ActualODEnd = $('#dtactualdeliveryend').val();

    var transporttypeid = $('#seltransporttype').val();
    var statusid = $('#selstatus').val();
    var Orderno = $('#txtshipmentno').val();
    var Jobno = $('#txtjobno').val();
    Model


    if (ActualODEnd != null && ActualODEnd != '') {
      
        Model.shipment_trip_plan.actual_delivery_end = ActualODEnd;
    }
    else {
        Model.shipment_trip_plan.actual_delivery_end = null;
    }

    if (ActualDOstart != null && ActualDOstart != '') {
       
        Model.shipment_trip_plan.actual_delivery_start = ActualDOstart;
    }
    else {
        Model.shipment_trip_plan.actual_delivery_start = null;
    }

    if (PlannedFrom != null && PlannedFrom != '') {
      
        Model.shipment_trip_plan.planned_from = PlannedFrom;
    }
    else {
        Model.shipment_trip_plan.planned_from = null;
    }

    if (PlannedTo != null && PlannedTo != '') {
       
        Model.shipment_trip_plan.planned_to = PlannedTo;
    }
    else {
        Model.shipment_trip_plan.planned_to = null;
    }

    Model.shipment_trip_plan.allowance_amt = driverallowance;
    Model.shipment_trip_plan.boarding_officer_name = BoaringOfficerName;
    Model.shipment_trip_plan.ctm = CTM;
    Model.shipment_trip_plan.driver_code = drivercode;
    Model.shipment_trip_plan.driver_contact_no = DriverContact;
    Model.shipment_trip_plan.driver_name = drivername;
    Model.shipment_trip_plan.estimate_packaging_unit = EstimatePackingUnit;
    Model.shipment_trip_plan.location_from = Locationfrom;
    Model.shipment_trip_plan.location_to = Locationto;
    Model.shipment_trip_plan.order_no = Orderno;
    Model.shipment_trip_plan.outsourced_contact_no = OutsourcedContactNo;
    Model.shipment_trip_plan.outsourced_contact_person = OutsourcedContactPerson;
    Model.shipment_trip_plan.outsourced_vehicle_no = OutsourcedVehicleNo;
    Model.shipment_trip_plan.shipment_statusid = statusid;
    Model.shipment_trip_plan.shipmentid = ShipmentId;
    Model.shipment_trip_plan.transport_company = transportCompany;
    Model.shipment_trip_plan.transport_type_id = transporttypeid;
    Model.shipment_trip_plan.vehicle_no = vehicalNo;
    Model.shipment_trip_plan.jobno = Jobno;
    Model.shipment_trip_plan.allowance_amt = driverallowance;


}
function saveOutboundShipment() {
    var res = false;
    GetCreatedShipmenInfo();
    GetCreatedtripPlan();
    if (IsDoPresent) {
        Model.shipment_Info.shipment_statusid = Outboundstatus['draft'];
    }
    Swal.fire({
        title: '', text: "Are you sure you want to create new Outbound Shipment ?",
        showCancelButton: true, confirmButtonText: 'Yes',
        cancelButtonText: 'No',
    }).then((result) => {
        if (result.isConfirmed) {
            SaveOutboundShipDetails();
        }
    });

    //return res;
}
function SaveOutboundShipDetails() {

    $.ajax({
        type: "POST",
        url: pathname + "/Outbound/SaveCreatedOutboundShipmentDetails",
        data: { "OutboundShipmentData": JSON.stringify(Model) },
        success: function (response) {
            try {
                if (response.result == true) {

                    Swal.fire({
                        text: "Outbound Shipment details saved successfully",
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
                        ViewDetails(response.shipmentid);

                    }));


                }
                else {
                    Swal.fire('Failed', 'Unable to save outbound shipment details. Message: ' + response.msg, 'error');

                }
            }
            catch (err) {
                Swal.fire('Failed', 'Exception while saving outbound shipment details. Exception: ' + err, 'error');

            }
        },
        error: function (response) {
            Swal.fire('Failed', 'Error while saving outbound shipment details. Exception: ' + response.responseText, 'error');

        }
    });

}

function ViewDetails(shipmentid) {
    $.ajax({
        url: pathname + '/Outbound/EncryptId',
        type: 'GET',
        data: { id: shipmentid },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.location = pathname + "/Outbound/EditOutboundShipmentDetails?ShipmentId=" + encryptedId;
            } else {
            }
        },
        error: function (xhr, status, error) {
        }
    });
}

function ViewDODetails(_id) {
    //console.log(_id)
    var dfgh = 0;
    $.ajax({
        url: '/Orders/EncryptId',
        type: 'GET',
        data: { id: _id },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.location = pathname + "/Orders/DeliveryOrderDetails?id=" + encodeURIComponent(encryptedId);
            } else {
                console.error("Error while encrypting ID: ", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error while encrypting ID: ", error);
        }
    });
}
function ClearModalFields() {
    $('#modallocation input').val('');
    $('#modallocation textarea').val('');
    $('#modallocation select').val('').trigger('change');
}
function GetOrderAttachment(deliveryOrderId) {

    $('#modalAttachment').modal({ backdrop: 'static', keyboard: false });
    ClearModalFields();

    var gh = 0;
    var count = 0;
    var _url = pathname + "/Orders/GetOrderAttachment";


    shipattachdoctable = $("#tblshipmentDODocuments").DataTable({
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
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 downloadDO" data-docid ="' + full.deliveryDocumentId + '" data-filename="' + full.documentName + '" href="#">Download</a>'
                }
            }


        ],
        "initComplete": function (data) {
            //$('#deliveryordersCount').text(data.aoData.length);
        },
        "language": {
            "emptyTable": 'There are no attachments linked!'
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

function DownloadAttachment(_shipmentId) {
    $.ajax({
        url: pathname + "/Orders/DownloadOrderAttachment?shipmentId=" + _shipmentId,
        type: "GET",
        success: function (response) {
            if (response.result === "SUCCESS") {
                // File found, create blob and initiate download
                //var _bytesArray = System.IO.File.ReadAllBytes(response.data.documentPath);
                //var _base64Data = Convert.ToBase64String(response.byteArray);
                var byteCharacters = atob(response.base64Data);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], { type: response.data.documentType });

                // Create temporary link to download the file
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = response.data.documentName; // Set the filename
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

