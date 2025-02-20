var tbl = null;
$(document).ready(function () { Initialize(); _detailpageLoaded = true; });

function Initialize() {
    $('#nav_userAdmin').addClass("show");
    $('#nav_userAdmin .menu-link').addClass("active");
    var isaccess = false;
   
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
            },
            {
                "targets": [4],
                "visible": isaccess
            }
            ],
        "pageLength": 10,
        "bDestroy": true
    });

        SearchData(oTable);
};
