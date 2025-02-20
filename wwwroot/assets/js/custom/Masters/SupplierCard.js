var oTable = null;
var CustomerId = 0;
$(document).ready(function () {
    $('#btnexportcustomercard').click(function () {
        PrintCustomerCard();
    });
    Initialize(); _detailpageLoaded = true;
});

function Initialize() {
    $('#nav_masters').addClass("show");
    $('#menuSuppliercard .menu-link').addClass("active");
    
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
            }
           ],
        "pageLength": 10,
        "bDestroy": true
    });

    SearchData(oTable);
};



function PrintCustomerCard() {
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
        url: pathname + "/Masters/ExportCustomerCard",
        data: {
            
            _customerId: CustomerId
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