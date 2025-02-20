var oDoTable = null;
var shipdoctable = null;
var shipattachdoctable = null;
var shipstatus = null;

var res = null;
$(document).ready(function () {
    
    $('#nav_OutboundShipments').addClass("show");
    $('.dateField').flatpickr({
        enableTime: false,
        dateFormat: "d/m/Y"

    });
    $('#selAssignDeliveryOrders').select2({
        
        width: '100%', // Ensure it takes the full width of the container
        closeOnSelect: false, // Keep dropdown open for multiple selections
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
    SetVesselDropdown();
    initSettabs();
    SetupShipmentInfo(ShipmentId);
    SetupTripDetails(ShipmentId);
    InitialiseDOTable(pathname + "/Outbound/GetDeliveryOrderList");
    LoadAttachments(pathname + "/Outbound/GetOutboundShipmentDocuments");

    uploadAttachment();
    $('#btnSaveoutboundshipment').click(function (e) {
        
        var selectedStatus = $('#selstatus').val();
        SaveOutboundShipmentDetails(selectedStatus);
    })

    $(document).on('click', '.Unassign-DO', function (e) {
        var _id = e.target.attributes['data-id'].value;
        var _code = e.target.attributes['data-code'].value;
        Swal.fire({
            title: '', text: "Are you sure you want to Unassign DO : '" + _code +"' from the shipment details?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                UnassignDeliveryOrders(_id);
            }
        });
    });
    $(document).on('click', '#btnAssignConfirm', function (e) {
        
        var SelectedIds = $('#selAssignDeliveryOrders').val();
        if (SelectedIds != undefined && SelectedIds != null && SelectedIds != "") {
            AssignDeliverOrders(SelectedIds);

        }
        else {
            Swal.fire('Failed', 'Please select delivery orders to assign!', 'error');
        }
        

    });
    $(document).on('click', '#btnassigndo', function (e) {           
        $("#selAssignDeliveryOrders option[value]").remove();
        LoadAssignEligibleDeliveryOrders(Model.shipment_Info.receiverid, Model.shipment_Info.jobno, Model.shipment_Info.vessel_name, Model.shipment_Info.vessel_eta);
        $('#AssignShipModal').modal('show');
    })
    $(document).on('click', '.do-download', function (e) {
        var _id = e.target.attributes['data-id'].value;
        GetOrderAttachment(_id);
    })
    $(document).on('click', '.downloadDO', function (e) {
        //var _id = e.target.attributes['data-docid'].value;
        var deliveryDocumentId = $(this).attr('data-docid');
        var DOId = $(this).attr('data-DOid');
        DownloadAttachment(deliveryDocumentId, DOId);
    })
    $(document).on('click', '.shipment-docc', function (e) {
        var _id = e.target.attributes['data-id'].value;
        DownloadOutboundAttachment(_id);
    })
    $(document).on('click', '.DO_DETAILS', function (e) {
        var _id = e.target.attributes['data-id'].value;
        ViewDeliveryDetails(_id);
    })
    $(document).on('click', '#btnReleaseShipment', function (e) {

        SaveOutboundShipmentDetails(Outboundstatus['ready to ship']);
    })
    $(document).on('click', '#btnCancelShipment', function (e) {
        SaveOutboundShipmentDetails(Outboundstatus['cancelled']);
    })
    $(document).on('click', '#btnUnReleaseShipment', function (e) {
        SaveOutboundShipmentDetails(Outboundstatus['draft']);
    })
    $(document).on('click', '.Doc-delete', function (e) {
        e.stopPropagation();
        var docid = e.target.attributes['data-docid'].value;
        var filename = e.target.attributes['data-filename'].value;
        if (docid != null && filename != null && docid != '' && filename != '' && filename != 'null') {
            Swal.fire({
                title: '', text: "Are you sure you want to remove shipment attachment : '"+filename+"' ?",
                showCancelButton: true, confirmButtonText: 'Yes',
                cancelButtonText: 'No',
            }).then((result) => {
                if (result.isConfirmed) {
                    RemoveDocfromtable(docid, filename);
                }
            });
            
        }
        else {
            Swal.fire('Failed', 'Unable to remove attachment', 'error');

        }
        

    })
    $('#tbshipmentinfo').click(function (e) {
        $('#tbshipmentinfo').css("background-color", "#40444d");
        $('#tbtransportOrder').css("background-color", "#787373");
        $('#tbdeliveryorder').css("background-color", "#787373");

        $('#kt_tab_pane_1').addClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbtransportOrder').click(function (e) {
        $('#tbtransportOrder').css("background-color", "#40444d");
        $('#tbshipmentinfo').css("background-color", "#787373");
        $('#tbdeliveryorder').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').addClass("show active");
        $('#kt_tab_pane_3').removeClass("show active");
    });
    $('#tbdeliveryorder').click(function (e) {
        $('#tbdeliveryorder').css("background-color", "#40444d");
        $('#tbshipmentinfo').css("background-color", "#787373");
        $('#tbtransportOrder').css("background-color", "#787373");

        $('#kt_tab_pane_1').removeClass("show active");
        $('#kt_tab_pane_2').removeClass("show active");
        $('#kt_tab_pane_3').addClass("show active");
    });

    //$('#btnPrintShipmentOrder').click(function (e) {
    //    PrintShipmentOrder();
    //});

    $('#btnPrintShipmentOrder').click(function () {
        $('#printModal').modal('show');
    });

    $('#confirmPrint').click(function () {
        PrintShipmentOrder();
        $('#printModal').modal('hide');
    });

    $('#previewbtn').click(function () {
        PreviewDocument();
        $('#printModal').modal('hide');
    });
    DisabledFieldAccordingStatus();
    AddRequiredMarkWhileRelease();
});

function AddRequiredMarkWhileRelease() {
    if (shipstatus == Outboundstatus['draft'] && shipdeliveryorders.length > 0) {
        $('.checkrequired').addClass('required');
    }
    else {
        $('.checkrequired').removeClass('required');
    }
}
function DisabledFieldAccordingStatus() {
    var BTN = document.getElementById("uploadButton");
    if (BTN == undefined || BTN == null) {
        $('#fileInput').attr('disabled', 'disabled');
    }
    if ((shipstatus != Outboundstatus['draft'] && shipstatus != Outboundstatus['new']) || UserDefaultData.companiesaccess[UserDefaultData.companyid][1]<2) {
        $('.tab-pane input, .tab-pane select').attr('disabled', 'disabled');
        $('.tab-pane textarea').attr('disabled', 'disabled');
        $('#uploadButton').remove();
        $('.tab-pane textarea').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });
        $('.datetimeField, .dateField').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });

        $('.tab-pane select[data-control="select2"], .tab-pane select:not([data-control="select2"])').siblings('.select2').children('.selection').children('.select2-selection').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });
        $('#selcustomer').attr('disabled', 'disabled');
        $('#selcustomer').siblings('.select2').children('.selection').children('.select2-selection').css({
            'background-color': '#eff2f5',
            'opacity': '1'
        });
       
    }
    $('#selstatus').attr('disabled', 'disabled');
    $('#selstatus').siblings('.select2').children('.selection').children('.select2-selection').css({
        'background-color': '#eff2f5',
        'opacity': '1'
    });
}
function LoadAssignEligibleDeliveryOrders(Receiverid, jobNo, vesselName, vesselEta) {

    var OptionData = Ajax(pathname + "/Outbound/GetDeliveryOrderToAssign", {
        "customerId": Receiverid, "jobno": jobNo, "vesselName": vesselName, "vesselETA": vesselEta
    }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            options += '<option value=' + OptionData.data[i].delivery_order_id + '>' + OptionData.data[i].delivery_order_no +'</option>';
        }
    }

    $('#selAssignDeliveryOrders')[0].innerHTML = options;
}
function AssignDeliverOrders(selectedDOS) {

    var _selShipment_No = Model.shipment_Info.order_no;
 
    if (ShipmentId != '' && selectedDOS != '') {
        $.ajax({
            type: "POST", async: false,
            url: pathname + "/Orders/AssignToShipment",
            data: { "id": ShipmentId, "deliveryOrderNos": selectedDOS },
            success: function (response) {
                try {
                    if (response.result == true) {
                        var msg = "Delivery orders assigned successfully to outbound shipment '" + _selShipment_No + "'!";

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
                            $('#selAssignDeliveryOrders').val('');
                            $('#AssignShipModal').modal('hide');
                            InitialiseDOTable(pathname + "/Outbound/GetDeliveryOrderList");
                            location.reload();
                        }));

                    }
                    else {
                        if (response.msg != '') {
                            Swal.fire('Validation Error !', 'Please check the selected Delivery Orders.', 'error');
                        }
                        else {
                            Swal.fire('Validation Error !', 'Please check the selected Delivery Orders.', 'error');
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
function UnassignDeliveryOrders(_id) {
    
    var _shipId = ShipmentId;
    $.ajax({
        type: "POST", async: false, url: pathname + "/Outbound/UnassignedDeliveryOrder", data: { "_DOId": _id, "_ShipmentId": _shipId },
        success: function (response) {
            try {
                if (response.result == true) {
                    Swal.fire({
                        text: "Delivery Order successfully unassigned!",
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
                        res = true;
                        InitialiseDOTable(pathname + "/Outbound/GetDeliveryOrderList");
                        location.reload();
                    }));
                    
                }
                else {
                    Swal.fire('Failed', 'Unable to Unassigned Delivery Order <br/> Message : ' + response.msg, 'error');
                    res = false;
                }
            }
            catch (err) {
                Swal.fire('Failed', 'Unable to Unassigned Delivery Order <br/> Exception : ' + err, 'error');
                res = false;
            }
        },
        error: function (response) {
            Swal.fire('Failed', 'Unable to Unassigned Delivery Order <br/> Exception : ' + response.responseText, 'error');
            res = false;
        }
    });
}
function initSettabs() {
    $('#tbshipmentinfo').css("background-color", "#40444d");
    $('#tbtransportOrder').css("background-color", "#787373");
    $('#tbdeliveryorder').css("background-color", "#787373");

    $('#kt_tab_pane_1').addClass("show active");
    $('#kt_tab_pane_2').removeClass("show active");
    $('#kt_tab_pane_3').removeClass("show active");
}
function GetOutboundShipmentInfo(shipmentId) {
    if (ShipmentId > 0) {
        var OptionData = Ajax(pathname + "/Outbound/GetShipmentInfo", { ShipmentId: shipmentId }, true);
        if (OptionData.data != undefined) {
            return OptionData.data;
        }
        else {
            return "";
        }

    }
}
function GetOutboundShipmentTripDetails(shipmentId) {
    if (ShipmentId > 0) {
        var OptionData = Ajax(pathname + "/Outbound/GetShipmentTripPlan", { ShipmentId: shipmentId }, true);
        if (OptionData.data != undefined) {
            return OptionData.data;
        }
        else {
            return "";
        }

    }
}
function SetupShipmentInfo(shipmentId) {
    var Data = GetOutboundShipmentInfo(shipmentId)
    if (Data != "" && Data != null) {


        if (Data.shipment_statusid != "" && Data.shipment_statusid != null) {

            $('#selstatus').val(Data.shipment_statusid);
            shipstatus = Data.shipment_statusid;
        }
        if (Data.order_no != "" && Data.order_no != null) {

            $('#txtshipmentno').val(Data.order_no);
        }
        if (Data.jobno != "" && Data.jobno != null) {

            $('#txtjobno').val(Data.jobno);
        }


        if (Data.vessel_eta != "" && Data.vessel_eta != null) {

            $('#txtvesseletadate').val(moment(new Date(Data.vessel_eta)).format('DD/MM/YYYY HH:mm'));
        }
        if (Data.receiverid != "" && Data.receiverid != null) {
            if (isbuyer) {
                $('#selcustomer option[data-company="' + Data.companyid + '"]').prop('selected', true);

            }
            else {
                $('#selcustomer').val(Data.receiverid);
            }
            
        }
        if (Data.transport_type_id != "" && Data.transport_type_id != null) {

            $('#seltransporttype').val(Data.transport_type_id);
        }
        if (Data.vessel_id != "" && Data.vessel_id != null) {
            $('#selvessel').val(Data.vessel_id);
        }
        //if (Data.vessel_code != "" && Data.vessel_code != null) {

        //    $('#txtvesselcode').val(Data.vessel_code);
        //}

        //if (Data.vessel_name != "" && Data.vessel_name != null) {

        //    $('#txtvesselname').val(Data.vessel_name);
        //}
        if (Data.vessel_ata != "" && Data.vessel_ata != null) {

            $('#txtvesselatadate').val(moment(new Date(Data.vessel_ata)).format('DD/MM/YYYY HH:mm'));
        }
        if (Data.anchorage_id != "" && Data.anchorage_id != null) {

            $('#selanchorage').val(Data.anchorage_id);
        }
        if (Data.agent != "" && Data.agent != null) {

            $('#txtagent').val(Data.agent);
        }
        if (Data.agent_contact_person != "" && Data.agent_contact_person != null) {

            $('#txtagentcontactperson').val(Data.agent_contact_person);
        }
        if (Data.loading_point != "" && Data.loading_point != null) {

            $('#txtloadingpoint').val(Data.loading_point);
        }
        if (Data.supply_boat != "" && Data.supply_boat != null) {

            $('#txtsupplyboat').val(Data.supply_boat);
        }
        if (Data.supply_boat_contact_person != "" && Data.supply_boat_contact_person != null) {

            $('#txtboatcontactperson').val(Data.supply_boat_contact_person);
        }
        if (Data.supply_boat_contact_no != "" && Data.supply_boat_contact_no != null) {

            $('#txtboatcontactno').val(Data.supply_boat_contact_no);
        }
        if (Data.agent_contact_no != "" && Data.agent_contact_no != null) {

            $('#txtagentcontact').val(Data.agent_contact_no);
        }
        if (Data.co_party != "" && Data.co_party != null) {

            $('#txtcoparty').val(Data.co_party);
        }
        if (Data.shipment_notes != "" && Data.shipment_notes != null) {

            $('#txtRemarks').val(Data.shipment_notes);
        }
        if (Data.delivery_date != "" && Data.delivery_date != null) {

            $('#dtdeliverydate').val(moment(new Date(Data.delivery_date)).format('DD/MM/YYYY HH:mm'));
        }

        if (Data.loading_time != "" && Data.loading_time != null) {

            $('#dtloadingtime').val(moment(new Date(Data.loading_time)).format('DD/MM/YYYY HH:mm'));
        }
        

    }
}

function SetupTripDetails(shipmentId) {
    var Data = GetOutboundShipmentTripDetails(shipmentId)
    if (Data != null && Data != "") {
        if (Data.boarding_officer_name != "" && Data.boarding_officer_name != null) {

            $('#txtboardingofficer').val(Data.boarding_officer_name);
        }
        if (Data.driver_code != "" && Data.driver_code != null) {

            $('#txtdrivercode').val(Data.driver_code);
        }
        if (Data.transport_company != "" && Data.transport_company != null) {

            $('#txttransportcompany').val(Data.transport_company);
        }
        if (Data.driver_name != "" && Data.driver_name != null) {

            $('#txtdrivername').val(Data.driver_name);
        }
        if (Data.outsourced_vehicle_no != "" && Data.outsourced_vehicle_no != null) {

            $('#txtxoutsourcedvehicleno').val(Data.outsourced_vehicle_no);
        }
        if (Data.outsourced_contact_person != "" && Data.outsourced_contact_person != null) {

            $('#txtOutsouecedcontactperson').val(Data.outsourced_contact_person);
        }
        if (Data.outsourced_contact_no != "" && Data.outsourced_contact_no != null) {

            $('#txtoutsourcedcontactno').val(Data.outsourced_contact_no);
        }
        if (Data.estimate_packaging_unit != "" && Data.estimate_packaging_unit != null) {

            $('#txtpackingunit').val(Data.estimate_packaging_unit);
        }
        if (Data.ctm != "" && Data.ctm != null) {

            $('#txctm').val(Data.ctm);
        }
        if (Data.location_from != "" && Data.location_from != null) {

            $('#txtfromloc').val(Data.location_from);
        }
        if (Data.location_to != "" && Data.location_to != null) {

            $('#txttoloc').val(Data.location_to);
        }
        if (Data.planned_from != "" && Data.planned_from != null) {

            $('#dtplannedstartdate').val(moment(new Date(Data.planned_from)).format('DD/MM/YYYY'));
        }
        if (Data.planned_to != "" && Data.planned_to != null) {

            $('#dtplannedenddate').val(moment(new Date(Data.planned_to)).format('DD/MM/YYYY'));
        }
        if (Data.driver_contact_no != "" && Data.driver_contact_no != null) {

            $('#txtDriercontact').val(Data.driver_contact_no);
        }
        if (Data.vehicle_no != "" && Data.vehicle_no != null) {

            $('#txtVehicalno').val(Data.vehicle_no);
        }
        if (Data.actual_delivery_start != "" && Data.actual_delivery_start != null) {

            $('#dtactualdeliverystart').val(moment(new Date(Data.actual_delivery_start)).format('DD/MM/YYYY'));
        }
        if (Data.actual_delivery_end != "" && Data.actual_delivery_end != null) {

            $('#dtactualdeliveryend').val(moment(new Date(Data.actual_delivery_end)).format('DD/MM/YYYY'));
        }
        if (Data.allowance_amt != "" && Data.allowance_amt != null) {

            $('#txtdriverallowance').val(Data.allowance_amt);
        }
    }

}
function GettimeFromDate(datetime) {
    var date = new Date(datetime);

    var hours = ('0' + date.getHours()).slice(-2);
    var minutes = ('0' + date.getMinutes()).slice(-2);

    // Format the time as HH:MM
    var time = hours + ':' + minutes;
    return time;
}

//#region Dropdowns
function SetStatusDropdown() {
    var OptionData = Ajax(pathname + "/Outbound/GetModuleStatuses", { "moduleId": 1 }, true);
    var options = '<option></option>'
    if (OptionData != undefined && OptionData.data != null && OptionData.data.length > 0) {
        for (var i = 0; i < OptionData.data.length; i++) {
            if (OptionData.data[i].statusid == Outboundstatus['new'] || OptionData.data[i].statusid == Outboundstatus['completed'] || OptionData.data[i].statusid == Outboundstatus['shipped']) {
                options += '<option disabled value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';

            }
            else {
                options += '<option value=' + OptionData.data[i].statusid + '>' + OptionData.data[i].status_desc + '</option>';

            }
        }
        Outboundstatus = OptionData.data.reduce((acc, item) => {
            acc[item.status_desc.toLowerCase()] = item.statusid;
            return acc;
        }, {});
    }
    $('#selstatus')[0].innerHTML = options;


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
//#endregion Dropdowns

//#region Datatables
function InitialiseDOTable(_url) {
    var isaccess = false;
    var isAttach = false;

    if (slModuleAction[1] >= AccessLevels.Write) {
        if (shipstatus == Outboundstatus['draft']) {
            isaccess = true;
        }
        
    }
    if (slModuleAction[1] >= AccessLevels.NoAccess) {
        isAttach = true;
    }
    oDoTable = $("#tblDeliveryorders").DataTable({
        "language": {
            "infoFiltered": ""
        },
        //"scrollY": '40vh',
        //"scrollCollapse": true,
        //"scrollX": true,
        "processing": true,
        "serverSide": true,
        "filter": true,
        "orderMulti": true,
        "order": [[1, "desc"]],
        "ordering":true,
        "lengthMenu": [
            [10, 25, 50, -1],
            ['10 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10,
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { ShipmentId: ShipmentId },
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
            { "visible": isaccess, "targets": 8 },
            { "visible": isAttach, "targets": 7 }
        ],
        "columns": [
            { "data": "delivery_order_id", "name": "", "autoWidth": true },
            {
                "data": "delivery_order_no", "name": "Delivery Order No", "autoWidth": true, "render": function (data, type, full, meta) {
                    return "<a href='#' class='DO_DETAILS' data-id=" + full.delivery_order_id + ">" + full.delivery_order_no + "</a>"
                }
            },
            { "data": "order_no", "name": "Order No", "autoWidth": true },
            { "data": "internal_dept", "name": "Internal Dept Code", "autoWidth": true },
            { "data": "dept_code", "name": "Customer Dept", "autoWidth": true },
            { "data": "do_status_desc", "name": "DO Status", "autoWidth": true },

            { "data": "sales_person_code", "name": "Sales Person Code", "autoWidth": true },
            {
                "data": "", "name": "DO Attachment", "autoWidth": true, "render": function (data, type, full, meta) {

                    return '<a href="#"><i class="fa fa-paperclip fs-3 me-2 do-download" data-id="' + full.delivery_order_id + '" aria-hidden="true"></i></a>';


                }

            },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<button class="btn btn-sm btn-success Unassign-DO" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end" data-code="' + full.delivery_order_no + '" data-id="' + full.delivery_order_id + '"><i class="fa fa-unlock fs-3 me-2"></i>' +
                        'Unassign' +
                        '</button>';
                }
            },


        ],
        "initComplete": function (data) {
   
            //if (data.aoData.length > 0) {
            //    $('#selcustomer').prop('disabled', true);
            //}
            //else {
            //    $('#selcustomer').prop('disabled', false);
            //}
            
            $('#deliveryordersCount').text(data.aoData.length);
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);
        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);

        }
    });
    //SearchData(oDoTable);
};
function LoadAttachments(_url) {
    var isaccess = false;
    if (slModuleAction[1] > AccessLevels.Write) {
        if (shipstatus == Outboundstatus['draft'] || shipstatus == Outboundstatus['new']) {
            isaccess = true;
        }
        
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
            "data": { ShipmentId: ShipmentId },
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
//#endregion Datatables
function GetExsitingAttchments() {
    var OptionData = Ajax(pathname + "/Outbound/GetOutboundShipmentDocuments", { shipmentid: ShipmentId }, true);
    populateExistingFiles(OptionData.data);

}
function populateExistingFiles(files) {
    const existingFilesList = document.getElementById('existingFilesList');
    files.forEach(file => {
        const listItem = document.createElement('li');
        listItem.className = 'list-group-item';
        const fileLink = document.createElement('a');
        fileLink.href = file.documentPath;
        fileLink.textContent = file.documentName;
        fileLink.target = '_blank'; // Open in a new tab

        listItem.appendChild(fileLink);

        existingFilesList.appendChild(listItem);
    });
}

//#region UpdateOutboundShipment
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
function GetUpdatedShipmenInfo() {
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
    if (shipstatus == Outboundstatus['new']) {
        Model.shipment_Info.shipment_statusid = Outboundstatus['draft'];
    }
    Model.shipment_Info.order_no = Orderno;
    Model.shipment_Info.jobno = Jobno;
    Model.shipment_Info.receiverid = customerid;
    Model.shipment_Info.transport_type_id = transporttypeid;
    //Model.shipment_Info.vessel_code = vesselcode;
    //Model.shipment_Info.vessel_name = vesselname;
    Model.shipment_Info.vessel_id = vesselid;
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
function GetUpdatedtripPlan(){

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


}
function getKeyByValue(object, value) {
    return Object.keys(object).find(key => object[key] === value);
}
function SaveOutboundShipmentDetails(statusid) {

    GetUpdatedShipmenInfo();
    GetUpdatedtripPlan();
    if (statusid > 0) {
        Model.shipment_Info.shipment_statusid = statusid;
    }
    if (shipstatus != Model.shipment_Info.shipment_statusid) { 
        var fromstatus = getKeyByValue(Outboundstatus, shipstatus);
        var tostatus = getKeyByValue(Outboundstatus, Number(Model.shipment_Info.shipment_statusid));
        if (Number(Model.shipment_Info.shipment_statusid) == Outboundstatus['draft']) {
            tostatus = "Unrelease";
        }
        if (Number(Model.shipment_Info.shipment_statusid) == Outboundstatus['ready to ship']) {
            tostatus = "Release";
        }
        if (Number(Model.shipment_Info.shipment_statusid) == Outboundstatus['cancelled']) {
            tostatus = "Cancel";
            Swal.fire({
                title: '',
                html: "Are you sure you want to '" + tostatus + "' Outbound Shipment : '" + Model.shipment_Info.order_no + "' ?<br><br>Please tell us why you are cancelling Shipment ? (Optional)",
                input: 'textarea',
                inputAttributes: {
                    autocapitalize: 'on'
                },
                showCancelButton: true,
                confirmButtonText: 'Confirm',
                cancelButtonText: 'No',
                showLoaderOnConfirm: true,
                customClass: {
                    popup: 'my-custom-dialog', // Custom class for the dialog
                },
                preConfirm: (reason) => {
                    // You can perform validation here if needed
                    return reason ? reason.charAt(0).toUpperCase() + reason.slice(1) : '';
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    let cancellationReason = result.value;
                    Model.shipment_Info.shipment_notes = cancellationReason;
                    if (UpdateDetails()) {
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

                            location.reload();
                        }));
                    }
                }
            });
        }
        else {

            Swal.fire({
                title: '', text: "Are you sure you want to '" + tostatus + "' Outbound Shipment : '" + Model.shipment_Info.order_no + "' ?",
                showCancelButton: true, confirmButtonText: 'Yes',
                cancelButtonText: 'No',
            })
                .then((result) => {
                    if (result.isConfirmed) {
                        //let cancellationReason = result.value;
                        //Model.shipment_Info.shipment_notes = cancellationReason;
                        if (UpdateDetails()) {
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

                                location.reload();
                            }));
                        }
                    }
                });
        }
    }
    else {
        if (shipstatus == Outboundstatus['new']) {
            Model.shipment_Info.shipment_statusid  = Outboundstatus['draft'];
        }
        Swal.fire({
            title: '', text: "Are you sure you want to save Outbound Shipment details : '" + Model.shipment_Info.order_no + "' ?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                if (UpdateDetails()) {
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

                        location.reload();
                    }));
                }
            }
        });
    }
    

}
function capitalizeFirstLetter(word) {
    if (typeof word !== 'string' || word.length === 0) {
        return '';
    }
    return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
}
function UpdateDetails() {
    var res = false;

    $.ajax({
        type: "POST",
        async: false,
        url: pathname + "/Outbound/SaveOutboundShipmentDetails",
        data: { "OutboundData": JSON.stringify(Model) },
        success: function (response) {
            try {
                if (response.result == true) {
                    res = true;
                }
                else {
                    Swal.fire('Failed', 'Unable to save outbound shipment Details <br/> Message : ' + response.msg, 'error');
                    res = false;
                }
            }
            catch (err) {
                Swal.fire('Failed', 'Unable to save  outbound shipment Details<br/> Exception : ' + err, 'error');
                res = false;
            }
        },
        error: function (response) {
            Swal.fire('Failed', 'Unable to save  outbound shipment Details<br/> Exception : ' + response.responseText, 'error');
            res = false;
        }
    });

    return res;
}
function uploadAttachment() {
    $('#uploadButton').on('click', function () {
        UploadDoc();
    });

    //$('#deleteButton').on('click', function () {
    //    RemoveDoc();
    //});
}
function UploadDoc() {
    var fileInput = $('#fileInput')[0];
    var file = fileInput.files[0];
    var shipmentId = ShipmentId;

    if (file && shipmentId) {

        if (isValidFileType(file.name)) {



            var formData = new FormData();
            formData.append('formFile', file);
            formData.append('shipmentId', shipmentId);

            $.ajax({
                url: pathname + '/Outbound/UploadAttachments',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    $('#uploadMessage').text(response.message);

                    if (response.success) {
                        Swal.fire({
                            text: "File successfully uploaded!",
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

                            $('#fileInput').val('');
                            LoadAttachments(pathname + "/Outbound/GetOutboundShipmentDocuments");

                        }));

                    }
                    else {
                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
                    }


                },
                error: function (xhr, status, error) {
                    console.error('Error:', error);
                    $('#uploadMessage').text('Unable to upload file.');
                }
            });
        }
        else {
            Swal.fire('Failed', 'Oops! This file type is not supported.Try another !', 'error');
            $('#uploadMessage').text('Oops! This file type is not supported.Try another !');
        }
    } else {
        $('#uploadMessage').text('No file is selected, Please choose file!');
    }
}
function isValidFileType(fileName) {
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'pdf','txt'];
    const fileExtension = fileName.split('.').pop().toLowerCase();
    return allowedExtensions.includes(fileExtension);
}
function RemoveDoc() {
    var filename = $(this).data('filename');
    var shipmentId = ShipmentId;
    var documentId = $(this).data('documentid');

    if (filename && shipmentId && documentId) {
        $.ajax({
            url: pathname+'/Outbound/DeleteAttachment',
            type: 'POST',
            data: { filename: filename, shipmentId: shipmentId, documentId: documentId },
            success: function (response) {
                $('#uploadMessage').text(response.message);
                if (response.success) {
                    //$('#deleteButton').hide();
                    LoadAttachments(pathname + "/Outbound/GetOutboundShipmentDocuments");
                    $('#fileInput').val('');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                $('#uploadMessage').text('Unable to delete file.');
            }
        });
    }
}
function RemoveDocfromtable(docid, filename) {

    var shipmentId = ShipmentId;


    try {
        if (filename && shipmentId && docid) {
            $.ajax({
                url: pathname+'/Outbound/DeleteAttachment',
                type: 'POST',
                data: { filename: filename, shipmentId: shipmentId, documentId: docid },
                success: function (response) {

                    if (response.success) {
                        Swal.fire({
                            text: "File successfully removed!",
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

                            LoadAttachments(pathname + "/Outbound/GetOutboundShipmentDocuments");

                        }));

                    }
                    else {
                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
                    }
                },
                error: function (xhr, status, error) {

                    Swal.fire('Failed', 'Unable to remove outbound shipment Document', 'error');
                }
            });
        }
    }
    catch (e) {
        Swal.fire('Failed', 'Unable to remove outbound shipment Document', 'error');
    }
    
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
            { "data": "deliveryOrderId", "name": "", "autoWidth": true },
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
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 downloadDO" data-docid ="' + full.deliveryDocumentId + '" data-DOid ="' + full.deliveryOrderId + '" data-filename="' + full.documentName + '" href="#">Download</a>'
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
function DownloadOutboundAttachment(documentId) {
    $.ajax({
        url: pathname + "/Outbound/DownloadshipmentAttachement",
        data: { documentid: documentId, shipmentid: ShipmentId },
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
function ViewDeliveryDetails(DeliveryOrderID) {
    $.ajax({
        url: pathname + '/Outbound/EncryptId',
        type: 'GET',
        data: { id: DeliveryOrderID },
        success: function (response) {
            if (response.success) {
                var encryptedId = response.encryptedId;
                window.location = pathname + "/Orders/DeliveryOrderDetails?id=" + encryptedId;
            } else {
            }
        },
        error: function (xhr, status, error) {
        }
    });
}
function DownloadAttachment(deliveryDocumentId, DOId) {
    $.ajax({
        url: pathname + "/Orders/DownloadOrderAttachment",
        data: { documentid: deliveryDocumentId, deliveryId: DOId },
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
function ClearModalFields() {
    $('#modallocation input').val('');
    $('#modallocation textarea').val('');
    $('#modallocation select').val('').trigger('change');
}
//#endregion

//#region Print OutBound Shipment
function getQueryParameter(name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
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
            _shipmentId: ShipmentId,
            printWithAllOrders: isCheckboxChecked,
            
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
            _shipmentId: ShipmentId,
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
//#endregion
