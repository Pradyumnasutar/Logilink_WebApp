
var result;
var oTable = null;
var shipdoctable = null;
var dopoporderattachment = null;
var DolinesTable = null;
var goodReturnCodes = [];
var CurrentDoidOpned = 0;
var isDoSavedFlag = false;
var isDoloaded = false;
var DeliveryOrderId = null;
$(document).ready(function () {

    $('#nav_EPOD').addClass("show");
    retainScrollBackonmodalchange();
    SetBreacrumbColors();
    InitialiseTable(pathname + "/EPOD/GetDeliveryOrderListforEpod");
    LoadAttachments(pathname + "/EPOD/GetOutboundShipmentDocuments");
    uploadAttachment();
    uploaddoAttachment();
    //Tabs Slider

    populateTabs()

    //Tabs Slider
    $(document).on('click', '.Doc-delete', function (e) {
        e.stopPropagation();
        var docid = e.target.attributes['data-docid'].value;
        var filename = e.target.attributes['data-filename'].value;
        if (docid != null && filename != null && docid != '' && filename != '' && filename != 'null') {
            Swal.fire({
                title: '', text: "Are you sure you want to remove shipment attachment : '" + filename + "' ?",
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

    $('#btnPrintShipmentOrder').click(function () {
        $('#printModal').modal('show');
    });

    $('#confirmPrint').click(function () {
        PrintEpodShipmentOrder();
        $('#printModal').modal('hide');
    });

    $('#previewbtn').click(function () {
        PreviewDocument();
        $('#printModal').modal('hide');
    });


    $(document).on('click', '.DO_DETAILS', function (e) {
        e.stopPropagation();
        var Doid = e.target.attributes['data-id'].nodeValue;
        CurrentDoidOpned = Doid;
        if (Doid > 0) {
            document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('show'));
            var tabLinkId = `popup-link-do-${Doid}`;
            document.getElementById(tabLinkId).classList.add('show', 'active');
            IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", Doid);
            GetPopupOrderAttachment(Doid);
            $('#dopopuploadMessage').text('');
            $('#dopopuploadMessage').val('');
            $('#modaldeliverylines').modal('show');
        }

    })
    $(document).on('click', '.do-attachment', function (e) {
        e.stopPropagation();
        var Doid = e.target.attributes['data-id'].nodeValue;

        if (Doid > 0) {

            GetOrderAttachment(Doid);
            $('#douploadMessage').text('');
            $('#modalDOAttachment').modal('show');
        }

    })
    $(document).on('click', '#btnPrintgrnreport', function (e) {
        e.stopPropagation();
        var ShipmentId = Model.shipmentInfo.shipmentid
        var isgrn = Model.shipmentInfo.finalReceiptExported
        if (ShipmentId > 0 && isgrn==1) {

            GenerateGoodsReturnReport(ShipmentId);
        }

    })
    $(document).on('click', '.btnDownloadDO', function () {
        var deliveryDocumentId = $(this).attr('data-docid');
        var DOId = $(this).attr('data-id');

        DownloadAttachment(deliveryDocumentId, DOId)
    });
    $(document).on('click', '#btnsave', function (e) {
        e.stopPropagation();
        var ShipmentRemarks = $('#txtRemarks').val();
        if (ShipmentRemarks != null && ShipmentRemarks != '') {
            SaveRemarks(ShipmentRemarks);
        }
        else {
            Swal.fire('Failed', 'please enter remarks!', 'error');
        }
    })
    $('#btnepodlogout').click(function () {
        LogOut();
    });
    $(document).on('click', '.btnremoveattach', function (e) {
        e.stopPropagation();
        var Doid = e.target.attributes['data-id'].nodeValue;
        var docid = e.target.attributes['data-docid'].value;
        var filename = e.target.attributes['data-filename'].value;
        if (Doid != null && Doid > 0 && docid != null && filename != null && docid != '' && filename != '' && filename != 'null') {
            Swal.fire({
                title: '', text: "Are you sure you want to remove delivery order attachment : '" + filename + "' ?",
                showCancelButton: true, confirmButtonText: 'Yes',
                cancelButtonText: 'No',
            }).then((result) => {
                if (result.isConfirmed) {
                    RemovedeliveryOrderDocfromtable(docid, Doid);

                }
            });

        }
        else {
            Swal.fire('Failed', 'Unable to remove attachment', 'error');

        }
    })
    $(document).on('click', '.btnremoveDodocfromline', function (e) {
        e.stopPropagation();
        var Doid = e.target.attributes['data-id'].nodeValue;
        var docid = e.target.attributes['data-docid'].value;
        var filename = e.target.attributes['data-filename'].value;
        if (Doid != null && Doid > 0 && docid != null && filename != null && docid != '' && filename != '' && filename != 'null') {
            Swal.fire({
                title: '', text: "Are you sure you want to remove delivery order attachment : '" + filename + "' ?",
                showCancelButton: true, confirmButtonText: 'Yes',
                cancelButtonText: 'No',
            }).then((result) => {
                if (result.isConfirmed) {
                    RemovedeliveryOrderDocfromLinetable(docid, Doid);

                }
            });

        }
        else {
            Swal.fire('Failed', 'Unable to remove attachment', 'error');

        }
    })
    $(document).on('click', '#btninitreceipt', function (e) {
        e.stopPropagation();
        Swal.fire({
            title: 'Are you sure?', icon: 'question', text: "Do you want to confirm initial receipt for Shipment : '" + Model.shipmentInfo.order_no + "' ?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                ConfirmStatus('ConfirmIntialReceiptshipment');
            }
        });

    })
    $(document).on('click', '#btnverifygoods', function (e) {
        e.stopPropagation();
        Swal.fire({
            title: 'Are you sure?', icon: 'question', text: "Do you want to confirm goods verification for Shipment : '" + Model.shipmentInfo.order_no + "' ?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                if (verifyCheckBoxes()) {
                    ConfirmStatus('ConfirmVerificationshipment');

                }

            }
        });

    })
    $(document).on('click', '#btnfinalrecipt', function (e) {
        e.stopPropagation();
        Swal.fire({
            title: 'Are you sure?', icon: 'question', text: "Do you want to confirm final receipt for Shipment : '" + Model.shipmentInfo.order_no + "' ?",
            showCancelButton: true, confirmButtonText: 'Yes',
            cancelButtonText: 'No',
        }).then((result) => {
            if (result.isConfirmed) {
                ConfirmStatus('ConfirmFinalReceipt');
            }
        });

    })
    $('.return-code').on('select2:open', function (e) {
        e.stopPropagation();
    });
    $(document).on('click', '.shipment-docc', function (e) {
        var _id = e.target.attributes['data-id'].value;
        DownloadOutboundAttachment(_id);
    })
    $(document).on('click', '.deliveryorder-doc', function (e) {
        var _id = e.target.attributes['data-id'].value;
        DownloadOutboundAttachment(_id);
    })
    loadGoodReturnCodelist();
});
//$(window).on('load', function (event) {
//    console.log('Page is fully loaded');
    //
//});
//#region DO Tabs

function LogOut() {
    NavigatePage("/Authenticate/EpodLogout");
}
function populateTabs() {
    var ShipmentId = Model.shipmentid;

    var deliveryOrders = Ajax(pathname + "/EPOD/GetDeliveryOrderListforEpodTabs", { "ShipmentId": ShipmentId }, true);
    deliveryOrders = deliveryOrders.value.data;
    const tabList = document.getElementById('dynamicTabList');
    const tabContentContainer = document.getElementById('dynamicTabContent');
    deliveryOrders.forEach((order, index) => {
        // Create the tab item
        const tabItem = document.createElement('li');
        tabItem.classList.add('nav-item');
        tabItem.classList.add('m-1');

        const tabLink = document.createElement('a');
        tabLink.classList.add('nav-link', 'btn', 'btn-active-light', 'btn-color-gray-600', 'btn-active-color-primary', 'rounded-bottom-0', 'do-tab');
        const tabLinkId = `link-do-${order.delivery_order_id}`;
        tabLink.id = tabLinkId;
        tabLink.href = `#do-${order.delivery_order_id}`;
        tabLink.setAttribute('data-bs-toggle', 'tab');
        tabLink.setAttribute('role', 'tab');
        tabLink.setAttribute('aria-controls', `do-${order.delivery_order_id}`);
        tabLink.setAttribute('aria-selected', 'false');
        tabLink.textContent = order.delivery_order_no;
        tabLink.style.background = "aliceblue";
        tabLink.style.color = "black";
        tabItem.appendChild(tabLink);
        tabList.appendChild(tabItem);

        // Create the tab content
        const tabContent = document.createElement('div');
        tabContent.classList.add('tab-pane', 'fade');
        tabContent.id = `do-${order.delivery_order_id}`;
        tabContent.setAttribute('role', 'tabpanel');
        tabContent.setAttribute('aria-labelledby', tabLinkId);
        tabContent.textContent = `Content for ${order.delivery_order_no}`;
        tabContent.style.background = "aliceblue";
        tabContent.style.color = "black";
        tabContentContainer.appendChild(tabContent);
    });

    // Add click event listeners to the new tabs
    document.querySelectorAll('.nav-link').forEach(tab => {
        tab.addEventListener('click', function () {
            setActiveTab(tab);
        });
    });
    populateTabsPopup(deliveryOrders);

}
$(document).on('click', '[id^="popup-link-do-"]', function (event) {
    event.preventDefault();
    // Your event handling logic here
    var id = $(this).attr('id');
    ActivePopupTab(id)
    // Additional logic based on the clicked element's ID
});
function truncateFileName(fileName, maxLength) {
    if (fileName.length <= maxLength) {
        return fileName;
    }

    const extension = fileName.substring(fileName.lastIndexOf('.')); // Get file extension
    const truncatedName = fileName.substring(0, maxLength - extension.length - 3); // Subtract length for "..."
    return truncatedName + '...';
}
function ActivePopupTab(doid) {
    var tabLinkId = '';
    if (!doid.includes('popup-link-do-')) {
        tabLinkId = `popup-link-do-${doid}`;
    }
    else {
        tabLinkId = doid;
    }
    var actualdoiid = tabLinkId.replace('popup-link-do-', '');
    $('#dopopuploadMessage').text('');
    $('#dopopuploadMessage').val('');
    var tab = document.getElementById(tabLinkId);
    document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('show'));
    document.querySelectorAll('.tab-pane').forEach(content => content.classList.remove('show', 'active'));
    document.getElementById(tabLinkId).classList.add('show', 'active');
    /*tab.classList.add('active');*/
    IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", actualdoiid);
    GetPopupOrderAttachment(actualdoiid);
}
function populateTabsPopup(deliveryOrders) {

    const tabList = document.getElementById('dynamicTabListPopup');
    const tabContentContainer = document.getElementById('dynamicTabContentPopup');

    deliveryOrders.forEach((order, index) => {
        // Create the tab item
        const tabItem = document.createElement('li');
        tabItem.classList.add('nav-item');

        const tabLink = document.createElement('a');
        tabLink.classList.add('nav-link', 'btn', 'btn-active-light', 'btn-color-gray-600', 'btn-active-color-primary', 'rounded-bottom-0', 'do-popup-tab');
        const tabLinkId = `popup-link-do-${order.delivery_order_id}`;
        tabLink.id = tabLinkId;
        tabLink.href = `#do-${order.delivery_order_id}`;
        tabLink.setAttribute('data-bs-toggle', 'tab');
        tabLink.setAttribute('role', 'tab');
        tabLink.setAttribute('aria-controls', `do-${order.delivery_order_id}`);
        tabLink.setAttribute('aria-selected', 'false');
        tabLink.textContent = order.delivery_order_no;

        tabItem.appendChild(tabLink);
        tabList.appendChild(tabItem);

        // Create the tab content
        const tabContent = document.createElement('div');
        tabContent.classList.add('tab-pane', 'fade');
        tabContent.id = `do-${order.delivery_order_id}`;
        tabContent.setAttribute('role', 'tabpanel');
        tabContent.setAttribute('aria-labelledby', tabLinkId);
        tabContent.textContent = `Content for ${order.delivery_order_no}`;

        tabContentContainer.appendChild(tabContent);
    });

}
function setActiveTab(tab) {
    document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.do-popup-tab').forEach(t => t.classList.remove('show'));
    document.querySelectorAll('.nav-link').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-pane').forEach(content => content.classList.remove('show', 'active'));

    const contentId = tab.getAttribute('href').substring(1);
    var doid = contentId.replace("do-", '');
    CurrentDoidOpned = doid;
    var popuptabLinkId = `popup-link-do-${doid}`;
    document.getElementById(popuptabLinkId).classList.add('show', 'active');
    document.getElementById(contentId).classList.add('show', 'active');
    tab.classList.add('active');
    IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", doid);
    GetPopupOrderAttachment(doid);
    $('#dopopuploadMessage').text('');
    $('#dopopuploadMessage').val('');
    $('#modaldeliverylines').modal('show');
}
function activateTab(tabId) {
    const tab = document.getElementById(tabId);
    if (tab) {
        setActiveTab(tab);
    }
}
//#endregion DO Tabs


//#region for Print Epod Shipment order

    function PrintEpodShipmentOrder() {
    var target = document.querySelector("#baseCard");
    var blockUI = new KTBlockUI(target, {
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
        url: pathname + "/EPOD/PrintShipmentOrders",
        data: {
            _shipmentId: Model.shipmentid,
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
        url: pathname + "/EPOD/PrintShipmentOrders",
        data: {
            _shipmentId: Model.shipmentid,
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

//#endregion for Print Epod Shipment order

//#region Print GRN report
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
//#endregion Print GRN report
function verifyCheckBoxes() {
    let allChecked = true;
    $('.verification-do').each(function () {
        if (!$(this).is(':checked')) {
            let code = $(this).data('code');
            Swal.fire('Error', 'Please verify and save delivery order details for :"' + code + '."', 'error');
            allChecked = false;
            return false; // Exit the .each loop
        }
    });
    return allChecked;
}
function ConfirmStatus(url) {
    $.ajax({
        url: pathname + '/EPOD/' + url,
        type: 'POST',
        //data: { shipmentid: shipmentid, deliveyrorderid: deliveryorderid, shipremark: shipremark, dolines: JSON.stringify(Dolines) },

        success: function (response) {

            if (response.result) {
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

                }));

            }
            else {
                Swal.fire('Failed', 'Message : ' + response.msg, 'error');
            }


        },
        error: function (xhr, status, error) {
            console.error('Error:', error);

        }
    });

}
function DownloadOutboundAttachment(documentId) {
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
        url: pathname + "/EPOD/DownloadshipmentAttachement",
        data: { documentid: documentId, shipmentid: Model.shipmentid },
        type: "GET",
        success: function (response) {
            blockUI.release();
            blockUI.destroy();
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
            blockUI.release();
            blockUI.destroy();
            // Handle other errors with SweetAlert
            swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again later.'
            });
        }
    });
}
function DownloadDeliveryOrderAttachment(deliveryDocumentId, DOId) {

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
        url: pathname + "/EPOD/DownloadOrderAttachment",
        data: { documentid: deliveryDocumentId, deliveryId: DOId },
        type: "GET",
        success: function (response) {
            blockUI.release();
            blockUI.destroy();
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

            blockUI.release();
            blockUI.destroy();            // Handle other errors with SweetAlert
            swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again later.'
            });
        }
    });
}
function DownloadAttachment(deliveryDocumentId, DOId) {
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
        url: pathname + "/EPOD/DownloadOrderAttachment",
        data: { documentid: deliveryDocumentId, deliveryId: DOId },
        type: "GET",
        success: function (response) {
            blockUI.release();
            blockUI.destroy();
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
            blockUI.release();
            blockUI.destroy();
            // Handle other errors with SweetAlert
            swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again later.'
            });
        }
    });
}
function loadGoodReturnCodelist() {
    $.ajax({
        url: pathname + '/EPOD/GetgoodreturnReasonlist',
        type: 'POST',
        /*data: { shipmentid: Model.shipmentid, remarks: remark },*/
        success: function (response) {

            if (response.result) {
                goodReturnCodes = response.data;
            }

        },
        error: function (xhr, status, error) {


        }
    });
}
function retainScrollBackonmodalchange() {
    var scrollPosition;

    $('#modaldeliverylines').on('show.bs.modal', function () {
        //$('.return-code').select2({
        //    dropdownAutoWidth: true,
        //    allowClear: true,
        //    width: 'resolve',
        //    dropdownParent: $('#modaldeliverylines')
        //});
        scrollPosition = $(window).scrollTop();
        $('body').css('overflow', 'hidden');
    });

    $('#modaldeliverylines').on('hidden.bs.modal', function () {
        $('body').css('overflow', 'auto');
        document.querySelectorAll('.nav-link').forEach(t => t.classList.remove('active'));
        $(window).scrollTop(scrollPosition);
    });
}
function SetBreacrumbColors() {
    var statusid = Model.shipmentInfo.shipment_statusid;
    if (statusid > 0) {
        if (statusid == 5) {//ready to ship
            $('firstbread,#secondbread,#thirdbread').removeClass('active');
            $('#firstbread,#secondbread,#thirdbread').removeClass('in-progress');
            $('#firstbread').addClass('in-progress');

        }
        if (statusid == 8) {//shipped
            $('firstbread,#secondbread,#thirdbread').removeClass('active');
            $('#firstbread,#secondbread,#thirdbread').removeClass('in-progress');
            $('#firstbread').addClass('active');
            $('#secondbread').addClass('in-progress');

        }
        if (statusid == 12) { //goods verified
            $('firstbread,#secondbread,#thirdbread').removeClass('active');
            $('#firstbread,#secondbread,#thirdbread').removeClass('in-progress');
            $('#firstbread').addClass('active');
            $('#secondbread').addClass('active');
            $('#thirdbread').addClass('in-progress');
        }
        if (statusid == 10) {//completed
            $('firstbread,#secondbread,#thirdbread').removeClass('active');
            $('#firstbread,#secondbread,#thirdbread').removeClass('in-progress');
            $('#firstbread').addClass('active');
            $('#secondbread').addClass('active');
            $('#thirdbread').addClass('active');
        }
    }
}
function SaveRemarks(remark) {
    if (remark != null && remark != '') {
        try {
            if (Model.shipmentid > 0) {
                $.ajax({
                    url: pathname + '/EPOD/SaveEpodRemark',
                    type: 'POST',
                    data: { shipmentid: Model.shipmentid, remarks: remark },
                    success: function (response) {

                        if (response.result) {
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
                                //LoadAttachments(pathname + "/Outbound/GetOutboundShipmentDocuments");

                            }));

                        }
                        else {
                            Swal.fire('Failed', response.msg, 'error');
                        }
                    },
                    error: function (xhr, status, error) {

                        Swal.fire('Failed', 'Unable to save ePOD shipment details', 'error');
                    }
                });
            }
            else {
                Swal.fire('Failed', 'Something went wrong! Please contact support for assistance.', 'error');
            }
        }
        catch (e) {
            Swal.fire('Failed', 'Something went wrong! Please contact support for assistance.', 'error');
        }

    }
    else {
        Swal.fire('Failed', 'please enter remarks!', 'error');
    }
}
function IntialiseDoLines(_url, Doid) {
    CurrentDoidOpned = Doid;
    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
    //if (slModuleAction[1002] > 3) {
    //    isaccess = true;
    //}
    var rdVal = Math.floor(Math.random() * 100) + 1;
    var count = 0;
    var customer = '';
    var nonoptions = '<option></option>';
    for (var i = 0; i < goodReturnCodes.length; i++) {
        nonoptions += '<option value=' + goodReturnCodes[i].grnReasonId + '>' + goodReturnCodes[i].grnReasonDescription + '</option>';
    }
    DolinesTable = $("#tbldeliveryorderlines").DataTable({
        "language": {
            "infoFiltered": ""
        },
        //"scrollY": '40vh',
        //"scrollCollapse": false,
        //"scrollX": true,
        "processing": true,
        "pagination": false,
        "serverSide": false,
        "filter": true,
        "orderMulti": false,
        "order": [[1, "asc"]],
        "lengthMenu": [
            [20, 25, 50, -1],
            ['20 rows', '25 rows', '50 rows', 'Show all']
        ],
        "pageLength": 10, // Depending on your requirement, you may set it to a different value
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { deliveryorderid: Doid },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {

            },

            "dataSrc": function (data) {
                customer = data.data._DOInfo.customer_name;
                isDoSavedFlag = data.data._DOInfo.do_verified_date ? true : false;
                $('#txtgoodsnotes').text(data.data._DOInfo.good_return_remarks);
                $('#txtgoodsnotes').val(data.data._DOInfo.good_return_remarks);
                return data.data._DOLines

            }
        },
        "columnDefs": [

            {
                "orderable": false, "targets": [0],
                "visible": false, "targets": [0]
            }

        ],
        "columns": [
            { "data": "delivery_order_lines_id", "name": "", "autoWidth": true },

            {
                "data": "", "name": "Row No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    count++;
                    return count;
                }
            },
            { "data": "item_no", "name": "Item No.", "autoWidth": true },

            {
                "data": "item_description", "name": "Customer Description", "autoWidth": true, "render": function (data, type, full, meta) {

                    return full.item_description;
                }
            },

            { "data": "dept_code", "name": "Customer Dept.", "autoWidth": true },
            { "data": "pono", "name": "Customer PO No.", "autoWidth": true },
            { "data": "packaging_unit_no", "name": "Packing Unit ID", "autoWidth": true },
            {
                "data": "", "name": "Deliveryed Qty", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<div data-id="' + full.delivery_order_lines_id + '" class="delivered-qty">' + roundDecimals(full.quantity) + '</div>'
                }
            },
            { "data": "uom", "name": "UOM", "autoWidth": true },
            {
                "data": "", "name": "Received Qty", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (Model.shipmentInfo.shipment_statusid == 8) {
                        if (isDoSavedFlag) {
                            return '<input type="number" value="' + roundDecimals(full.quantity_invoiced) + '" data-id="' + full.delivery_order_lines_id + '" class="rec-qty form-control form-control-sm fs-7 text-dark " oninput="if(this.value<0) this.value=0 ;"  onkeypress="return (event.charCode!=45 && event.charCode!=69 && event.charCode!=101)" placeholder="Received Quantity" />';

                        }
                        else {
                            return '<input type="number" value="' + roundDecimals(full.quantity) + '" data-id="' + full.delivery_order_lines_id + '" class="rec-qty form-control form-control-sm fs-7 text-dark " oninput="if(this.value<0) this.value=0 ;"  onkeypress="return (event.charCode!=45 && event.charCode!=69 && event.charCode!=101)" placeholder="Received Quantity" />';

                        }
                    }
                    else {
                        if (full.quantity_invoiced > 0) {
                            return roundDecimals(full.quantity_invoiced);
                        }
                        else {
                            return 0;
                        }

                    }

                }
            },
            {
                "data": "epod_line_remarks", "name": "Remark", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (Model.shipmentInfo.shipment_statusid == 8) {
                        return '<input type="text" value="' + (full.epod_line_remarks || '') + '" data-id="' + full.delivery_order_lines_id + '" class="form-control form-control-sm fs-7 text-dark remark" placeholder="Remarks" />';
                    } else {
                        return full.epod_line_remarks || '';
                    }

                }
            },
            {
                "data": "grn_reason_code", "name": "Reason Code", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (Model.shipmentInfo.shipment_statusid == 8) {
                        if (full.grn_reason_id != undefined && full.grn_reason_id > 0) {
                            var options = '<option></option>';
                            for (var i = 0; i < goodReturnCodes.length; i++) {
                                if (full.grn_reason_id == goodReturnCodes[i].grnReasonId) {
                                    options += '<option Selected value=' + goodReturnCodes[i].grnReasonId + '>' + goodReturnCodes[0].grnReasonCode + " - " + goodReturnCodes[i].grnReasonDescription + '</option>';

                                }
                                else {
                                    options += '<option value=' + goodReturnCodes[i].grnReasonId + '>' + goodReturnCodes[0].grnReasonCode + " - " + goodReturnCodes[i].grnReasonDescription + '</option>';

                                }
                            }
                            return ' <select data-placeholder="Select return code" data-hidden-id="' + full.grn_reason_id + '" data-id="' + full.delivery_order_lines_id + '" data-control="select2" class="form-select fs-7 return-code" tabindex="-1">' + options + '</select > '

                        }
                        else {
                            return ' <select data-placeholder="Select return code" data-id="' + full.delivery_order_lines_id + '" data-control="select2" class="form-select fs-7 return-code" tabindex="-1">' + nonoptions + '</select > '

                        }
                    }
                    else {
                        if (full.grn_reason_code !== null && full.grn_reason_description !== null) {
                            return full.grn_reason_code + " - " + full.grn_reason_description;
                        }
                        else {
                            return " - ";
                        }

                    }

                }
            },
            {
                "data": "", "name": "Good Return", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (Model.shipmentInfo.shipment_statusid == 8) {
                        if (full.quantity_invoiced != full.quantity && isDoSavedFlag) {
                            var valcal = roundDecimals(full.quantity) - roundDecimals(full.quantity_invoiced)
                            return '<input type="numeber" value="' + valcal + '" data-id="' + full.delivery_order_lines_id + '" class="form-control form-control-sm fs-7 text-dark return-qty" placeholder="good returns" disabled="disabled" />';

                        }
                        else {
                            return '<input type="numeber" value="0" data-id="' + full.delivery_order_lines_id + '" class="form-control form-control-sm fs-7 text-dark return-qty" placeholder="good returns" disabled="disabled" />';

                        }
                    }
                    else {
                        if (full.quantity_invoiced != full.quantity && full.quantity_invoiced != 0) {
                            var valcal = roundDecimals(full.quantity) - roundDecimals(full.quantity_invoiced)
                            if (valcal > 0) {
                                return '<div class="disabled-red" style="text-align: center;">' + valcal + '</div>';
                            }
                            else {
                                return '<div class=""style="text-align: center;">' + valcal + '</div>';
                            }
                        }
                        else {
                            if (full.grnReasonId > 0) {
                                var valcal = roundDecimals(full.quantity) - roundDecimals(full.quantity_invoiced)
                                if (valcal > 0) {
                                    return '<div class="disabled-red" style="text-align: center;">' + valcal + '</div>';
                                }
                                else {
                                    return '<div class=""style="text-align: center;">' + valcal + '</div>';
                                }
                            }
                            else {
                                return '<div class=""style="text-align: center;">0</div>';
                            }

                        }

                    }
                }
            }

        ],
        "initComplete": function (data) {

            //if (Model.shipmentInfo.shipment_statusid == 8) {
            //    SetGoodreturnDropdown();
            //}
            //$('#lblTotalRecord').text(data._iRecordsTotal);
            $('.return-code').select2({
                dropdownParent: $('#modaldeliverylines'),
                dropdownAutoWidth: true,
                width: '100%',
                allowClear: true

            });

            applyClassBasedOnValue();


        },
        "drawCallback": function (settings) {
            setTimeout(function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            }, 100);


        }

    });

}
function applyClassBasedOnValue() {
    $('.return-qty').each(function () {
        if ($(this).val() > 0) {
            $(this).addClass('disabled-red');
        } else {
            $(this).removeClass('disabled-red');
        }
        if ($(this).val() == 0) {
            $(this)
                .closest('tr') // Assuming the .return-qty and select are within the same row (tr)
                .find('.return-code') // Locate the select dropdown with the class 'return-code'
                .val(null) // Remove the selected option
                .trigger('change');
        }
    });
}
function SetGoodreturnDropdown() {
    var options = '<option></option>';
    for (var i = 0; i < goodReturnCodes.length; i++) {
        options += '<option value=' + goodReturnCodes[i].grnReasonId + '>' + goodReturnCodes[i].grnReasonDescription + '</option>';
    }
    //$('.return-code')[0].innerHTML = options;
    var selects = document.querySelectorAll('.return-code');

    selects.forEach(function (select) {
        select.innerHTML = options;
    });

    $('.return-code').select2({
        dropdownParent: $('#modaldeliverylines'),
        dropdownAutoWidth: true,
        width: '100%',
        allowClear: true

    });

}
function InitialiseTable(_url) {
    var ShipmentId = Model.shipmentid;

    var isaccess = false; // Depending on your requirement, you might want to make this dynamic
    //if (slModuleAction[1002] > 3) {
    //    isaccess = true;
    //}
    var rdVal = Math.floor(Math.random() * 100) + 1;
    oTable = $("#tbldeliveryorders").DataTable({
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
            [20, 25, 50, -1],
            ['20 rows', '25 rows', '50 rows', 'Show all']
        ],
        "bDestroy": true,
        "ajax": {
            "url": _url,
            "data": { ShipmentId: ShipmentId },
            "type": "POST",
            "datatype": "json",
            "error": function (XMLHttpRequest, textStatus, errorThrown) {

            },

            "dataSrc": function (data) {
                if (!isDoloaded) {
                    isDoloaded = true;


                }
                else {

                }

                return data.data;
            }
        },
        "columnDefs": [

            {
                "orderable": false, "targets": [0],
                "visible": false, "targets": [0]
            }

        ],
        "columns": [
            { "data": "delivery_order_id", "name": "", "autoWidth": true },
            {
                "data": "", "name": "Row No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    if (full.do_verified_date != null && full.do_verified_date != undefined) {

                        return '<label class="checkbox checkbox-success"><input type = "checkbox" class="verification-do" disabled="disabled" data-code="' + full.delivery_order_no + '" name = "Checkboxes5" checked = "checked"></label > ';

                    }
                    else {
                        return '<label class="checkbox checkbox-success"><input type = "checkbox" class="verification-do" disabled="disabled" data-code="' + full.delivery_order_no + '" name = "Checkboxes5"></label >'

                    }

                }
            },
            {
                "data": "delivery_order_no", "name": "Delivery Order No", "autoWidth": true, "render": function (data, type, full, meta) {
                    return "<a href='#' class='DO_DETAILS' data-id=" + full.delivery_order_id + ">" + full.delivery_order_no + "</a>"
                }
            },
            { "data": "packaging_unit_no", "name": "Packing Unit ID", "autoWidth": true },
            { "data": "order_no", "name": "Shipment No.", "autoWidth": true },
            { "data": "dept_code", "name": "Customer Dept", "autoWidth": true },
            { "data": "pono", "name": "Customer PO No.", "autoWidth": true },
            { "data": "packaging_unit_no", "name": "Packing Units", "autoWidth": true },
            { "data": "do_status_desc", "name": "DO Status", "autoWidth": true },

            { "data": "sales_person_code", "name": "Sales Person Code", "autoWidth": true },
            {
                "data": "", "name": "Attachment", "autoWidth": true, "render": function (data, type, full, meta) {

                    return '<a href="#"><i class="fa fa-paperclip fs-3 me-2 do-attachment" data-id="' + full.delivery_order_id + '" aria-hidden="true"></i></a>';


                }

            }

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

        }

    });

    SearchData(oTable);
}
function roundDecimals(value) {
    if (value > 0 || value < 0) {
        return Number(value.toFixed(4));
    }
    return value;
}
function LoadAttachments(_url) {
    var isaccess = false;
    var count = 0
    if (slModuleAction[3] > AccessLevels.Write) {
        if (Model.shipmentInfo.shipment_statusid != 10) {
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
            "data": { ShipmentId: Model.shipmentid },
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
            { "visible": isaccess, "targets": 3 }
        ],
        "columns": [
            { "data": "shipmentDocumentId", "name": "", "autoWidth": true },
            {
                "data": "", "name": "No.", "autoWidth": true, "render": function (data, type, full, meta) {
                    return count = count + 1;
                }
            },
            {
                "data": "", "name": "Attachments", "autoWidth": true, "render": function (data, type, full, meta) {
                    return "<a href='#' class='shipment-docc' data-id='" + full.shipmentDocumentId + "' title='" + full.documentName + "'>" + truncateFileName(full.documentName, 30) + "</a>"

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
function uploadAttachment() {
    $('#uploadButton').on('click', function () {
        UploadDoc();
    });

}
function uploaddoAttachment() {
    $('#douploadButton').on('click', function (e) {
        var Doid = e.target.attributes['data-id'].nodeValue;
        UploaddoDoc(Doid);
    });
    $('#dopopuploadButton').on('click', function (e) {
        var Doid = e.target.attributes['data-id'].nodeValue;
        UploadpopdoDoc(Doid);
    });

}
function UploaddoDoc(Doid) {
    var fileInput = $('#dofileInput')[0];
    var file = fileInput.files[0];


    if (file && Doid) {

        if (isValidFileType(file.name)) {
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

            var formData = new FormData();
            formData.append('formFile', file);
            formData.append('Doid', Doid);

            $.ajax({
                url: pathname + '/EPOD/UploaddoAttachments',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    $('#douploadMessage').text(response.message);
                    blockUI.release();
                    blockUI.destroy();
                    if (response.success) {
                        Swal.fire({
                            text: response.message,
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

                            $('#dofileInput').val('');

                            if (Doid > 0) {

                                GetOrderAttachment(Doid);

                            }


                        }));

                    }
                    else {
                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
                    }


                },
                error: function (xhr, status, error) {
                    blockUI.release();
                    blockUI.destroy();
                    console.error('Error:', error);
                    $('#douploadMessage').text('Unable to upload file.');
                }
            });
        }
        else {
            Swal.fire('Failed', 'Oops! This file type is not supported.Try another !', 'error');
            $('#douploadMessage').text('Oops! This file type is not supported.Try another !');
        }
    } else {
        Swal.fire('Warning!', 'No file is selected, Please choose file!', 'warning');
        $('#douploadMessage').text('No file is selected, Please choose file!');
    }
}
function UploadpopdoDoc(Doid) {
    var fileInput = $('#dopopfileInput')[0];
    var file = fileInput.files[0];


    if (file && Doid) {

        if (isValidFileType(file.name)) {

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


            var formData = new FormData();
            formData.append('formFile', file);
            formData.append('Doid', Doid);

            $.ajax({
                url: pathname + '/EPOD/UploaddoAttachments',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    $('#dopopuploadMessage').text(response.message);
                    blockUI.release();
                    blockUI.destroy();
                    if (response.success) {
                        Swal.fire({
                            text: response.message,
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

                            $('#dopopfileInput').val('');

                            if (Doid > 0) {

                                GetPopupOrderAttachment(Doid);

                            }


                        }));

                    }
                    else {
                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
                    }


                },
                error: function (xhr, status, error) {
                    blockUI.release();
                    blockUI.destroy();
                    console.error('Error:', error);
                    $('#dopopuploadMessage').text('Unable to upload file.');
                }
            });
        }
        else {
            Swal.fire('Failed', 'Oops! This file type is not supported.Try another !', 'error');
            $('#dopopuploadMessage').text('Oops! This file type is not supported.Try another !');
        }
    } else {
        Swal.fire('Warning!', 'No file is selected, Please choose file!', 'warning');
        $('#dopopuploadMessage').text('No file is selected, Please choose file!');
    }
}
function UploadDoc() {
    var fileInput = $('#fileInput')[0];
    var file = fileInput.files[0];
    var shipmentId = Model.shipmentid;

    if (file && shipmentId) {
        if (isValidFileType(file.name)) {
            var target = document.querySelector("#baseCard");
            var blockUI = new KTBlockUI(target, {
                overlayClass: "bg-white bg-opacity-10"
            });
            blockUI.block();

            var formData = new FormData();
            formData.append('formFile', file);
            formData.append('shipmentId', shipmentId);

            $.ajax({
                url: pathname + '/EPOD/UploadAttachments',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    $('#uploadMessage').text(response.message);
                    blockUI.release();
                    blockUI.destroy();
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
                            LoadAttachments(pathname + "/EPOD/GetOutboundShipmentDocuments");
                        }));
                    } else {
                        Swal.fire('Failed', 'Message : ' + response.message, 'error');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error:', error);
                    $('#uploadMessage').text('Unable to upload file.');
                    blockUI.release();
                    blockUI.destroy();
                }
            });
        } else {
            Swal.fire('Failed', 'Oops! This file type is not supported. Try another!', 'error');
            $('#uploadMessage').text('Oops! This file type is not supported. Try another!');
        }
    } else {
        $('#uploadMessage').text('No file is selected, Please choose a file!');
    }
}

function isValidFileType(fileName) {
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'pdf', 'txt'];
    const fileExtension = fileName.split('.').pop().toLowerCase();
    return allowedExtensions.includes(fileExtension);
}
function RemoveDocfromtable(docid, filename) {

    var shipmentId = Model.shipmentid;


    try {
        if (filename && shipmentId && docid) {
            $.ajax({
                url: pathname + '/EPOD/DeleteAttachment',
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

                            LoadAttachments(pathname + "/EPOD/GetOutboundShipmentDocuments");

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
function RemovedeliveryOrderDocfromtable(docid, deliveryorderid) {
    var res = false;

    try {
        if (docid > 0) {
            $.ajax({
                url: pathname + '/EPOD/RemoveDeliveryOrderDocumnet',
                type: 'POST',
                data: { deliverydocumentid: docid, deliveryorderid: deliveryorderid },
                success: function (response) {

                    if (response.result) {
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

                            GetOrderAttachment(deliveryorderid);
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
        Swal.fire('Failed', 'Unable to remove delivery order Document', 'error');
    }
    return res;

}
function RemovedeliveryOrderDocfromLinetable(docid, deliveryorderid) {
    var res = false;

    try {
        if (docid > 0) {
            $.ajax({
                url: pathname + '/EPOD/RemoveDeliveryOrderDocumnet',
                type: 'POST',
                data: { deliverydocumentid: docid, deliveryorderid: deliveryorderid },
                success: function (response) {

                    if (response.result) {
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

                            GetPopupOrderAttachment(deliveryorderid);
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
        Swal.fire('Failed', 'Unable to remove delivery order Document', 'error');
    }
    return res;

}

//#region Delivery Order Line Popup Functions
$(document).on('click', '#btnSaveLines', function (e) {
    e.stopPropagation();
    if (CurrentDoidOpned > 0) {
        // Step 1: Store Current Pagination Information
        var pageInfo = DolinesTable.page.info();
        var currentPage = pageInfo.page;
        var currentLength = pageInfo.length;

        // Step 2: Change Pagination to Show All Rows
        DolinesTable.page.len(-1).draw();

        var ShipmentRemarks = $('#txtgoodsnotes').val();
        var deliveredqty = $('.delivered-qty');
        var receivedqty = $('.rec-qty');
        var returnqty = $('.return-qty');
        var remarks = $('.remark');
        var goodreturncode = $('.return-code').filter(function () {
            return $(this).attr('data-id') !== undefined;
        });

        var result = {};

        // Function to populate the result object
        function populateResult(elements, valueKey) {
            elements.each(function () {
                var dataId = $(this).attr('data-id');
                if (dataId !== undefined && dataId !== null) {
                    var value = $(this).val();
                    if (!result[dataId]) {
                        result[dataId] = {};
                    }
                    result[dataId][valueKey] = value;
                }
            });
        }
        function populateqtyResult(elements, valueKey) {
            elements.each(function () {
                var dataId = $(this).attr('data-id');
                if (dataId !== undefined && dataId !== null) {
                    var value = $(this).val() || $(this).text();
                    if (!result[dataId]) {
                        result[dataId] = {};
                    }
                    result[dataId][valueKey] = value;
                }
            });
        }
        // Populate the result object with data from each category
        populateqtyResult(deliveredqty, 'deliveredQty');
        populateqtyResult(receivedqty, 'receivedQty');
        populateqtyResult(returnqty, 'returnQty');
        populateResult(remarks, 'remark');
        populateResult(goodreturncode, 'returnCode');

        // Step 4: Restore Previous Pagination Settings
        DolinesTable.page.len(currentLength).draw();
        DolinesTable.page(currentPage).draw(false);

        SaveDeliveryorderLines(Model.shipmentid, CurrentDoidOpned, ShipmentRemarks, result);
    } else {
        Swal.fire('error', 'Something went wrong!', 'error');
    }
});
$(document).on('input', '.rec-qty', function (e) {
    e.stopPropagation();

    var dolineid = Number(e.target.attributes["data-id"].nodeValue)

    if (dolineid > 0) {
        var ReceivedqtyVal = Number(e.target.value);
        var deliveredqty = $('div[data-id="' + dolineid + '"].delivered-qty');
        var returnqty = $('input[data-id="' + dolineid + '"].return-qty');
        var remarks = $('input[data-id="' + dolineid + '"].remark');
        var goodreturncode = $('select[data-id="' + dolineid + '"].return-code');
        if (deliveredqty.length > 0 && returnqty.length > 0 && remarks.length > 0 && goodreturncode.length > 0) {
            var valDeliveryqty = Number(deliveredqty[0].innerText);
            var calval = valDeliveryqty - ReceivedqtyVal;
            $('input[data-id="' + dolineid + '"].return-qty').val(calval);
            applyClassBasedOnValue();
        }
        else {
            Swal.fire('error', 'Something went wrong!', 'error');
        }

        var fhf = 0;

    }
    else {
        Swal.fire('error', 'Something went wrong!', 'error');
    }

})
function SaveDeliveryorderLines(shipmentid, deliveryorderid, DOremark, Dolines) {
    if (shipmentid > 0 && deliveryorderid > 0 && Dolines != undefined) {

        $.ajax({
            url: pathname + '/EPOD/SaveDeliveryOrderLines',
            type: 'POST',
            data: { shipmentid: shipmentid, deliveyrorderid: deliveryorderid, shipremark: DOremark, dolines: JSON.stringify(Dolines) },

            success: function (response) {

                if (response.result) {
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

                        IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", deliveryorderid);
                        InitialiseTable(pathname + "/EPOD/GetDeliveryOrderListforEpod");

                    }));

                }
                else {
                    Swal.fire('Failed', 'Message : ' + response.msg, 'error');
                }


            },
            error: function (xhr, status, error) {
                console.error('Error:', error);

            }
        });

    } else {
        Swal.fire('Failed', 'Something went wrong, unable to save details!', 'error');
    }
}
$(document).on('click', '.DO-slider', function (e) {
    e.stopPropagation();
    var Doid = e.target.attributes['data-id'].nodeValue
    if (Doid > 0) {
        CurrentDoidOpned = Doid;
        IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", Doid);
        GetPopupOrderAttachment(Doid);
        $('#modaldeliverylines').modal('show');
    }
    else {
        Swal.fire('error', 'Oops, Something went wrong!', 'error');
    }
})

$(document).on('click', '.DO-sliderline', function (e) {
    e.stopPropagation();
    var Doid = e.target.attributes['data-id'].nodeValue
    if (Doid > 0) {
        CurrentDoidOpned = Doid;
        IntialiseDoLines(pathname + "/EPOD/GetDeliveryOrderLinesByDeliveryOrder", Doid);
        GetPopupOrderAttachment(Doid);
    }
    else {
        Swal.fire('error', 'Oops, Something went wrong!', 'error');
    }
})
//#endregion Delivery Order Line Popup Functions

//#region deliveryorder popup

//#endregion deliveryorder popup

//#region Delivery order attachments

function GetOrderAttachment(deliveryOrderId) {

    $('#modalDOAttachment').modal({ backdrop: 'static', keyboard: false });

    var gh = 0;
    var count = 0;
    var _url = pathname + "/EPOD/GetOrderAttachment";
    var isaccess = false;
    if (slModuleAction[3] > AccessLevels.Write) {
        if (Model.shipmentInfo.shipment_statusid != 10) {
            isaccess = true;
        }
    }


    shipdoctable = $("#tbldoDocuments").DataTable({
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
            { "visible": false, "targets": 0 },
            { "visible": isaccess, "targets": 3 }
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
                    return "<a href='#'class='btnDownloadDO' data-docid=" + full.deliveryDocumentId + " data-id=" + full.deliveryOrderId + " title='" + full.documentName + "' >" + truncateFileName(full.documentName, 30) + "</a>"
                    //return full.documentName;
                }
            },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnremoveattach" data-docid ="' + full.deliveryDocumentId + '" data-id ="' + full.deliveryOrderId + '" data-filename="' + full.documentName + '" href="#">Remove</a>'
                }
            }


        ],
        "initComplete": function (data) {
            $('#douploadButton').attr("data-id", deliveryOrderId);
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

    $('#modalDOAttachment').modal('show');
}
function GetPopupOrderAttachment(deliveryOrderId) {

    //$('#modalDOAttachment').modal({ backdrop: 'static', keyboard: false });

    var gh = 0;
    var count = 0;
    var _url = pathname + "/EPOD/GetOrderAttachment";
    var isaccess = false;
    if (slModuleAction[3] > AccessLevels.Write) {
        if (Model.shipmentInfo.shipment_statusid != 10) {
            isaccess = true;
        }
    }

    dopoporderattachment = $("#tbldopoporderattachments").DataTable({
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
            { "visible": false, "targets": 0 },
            { "visible": isaccess, "targets": 3 }
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
                    return "<a href='#'class='btnDownloadDO' data-docid=" + full.deliveryDocumentId + " data-id=" + full.deliveryOrderId + " title='" + full.documentName + "'>" + truncateFileName(full.documentName, 30) + "</a>"

                }
            },
            {
                "data": "", "name": "Action", "autoWidth": true, "render": function (data, type, full, meta) {
                    return '<a class="btn btn-sm btn-link btn-color-danger btn-active-color-danger me-5 btnremoveDodocfromline" data-docid ="' + full.deliveryDocumentId + '" data-id ="' + full.deliveryOrderId + '" data-filename="' + full.documentName + '" href="#">Remove</a>'
                }
            }


        ],
        "initComplete": function (data) {
            $('#dopopuploadButton').attr("data-id", deliveryOrderId);
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
}
//#endregion Delivery order attachments
var SearchData = (oTable) => {
    const filterSearch = document.querySelector('[data-kt-filter="search"]');
    filterSearch.addEventListener('keyup', function (e) {
        oTable.search(e.target.value).draw();
    });
}