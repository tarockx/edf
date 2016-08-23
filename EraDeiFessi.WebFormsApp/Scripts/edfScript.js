/// <reference path="jquery-1.9.1.js" />


function moveLinks(elmnt) {
    $('#linksOperationsBody').empty(); //remove old links
    var lista = $(elmnt).parent().find("ul").clone(); //get new links
    $(lista).appendTo('#linksOperationsBody'); //put new links in the popup

    //open modal while at the same time fixing the size error
    $('#linksOperations').on('shown.bs.modal', function (e) {
        //fix size
        var h = $("#wrap").height();
        console.log(h);
        $(".modal-backdrop").height(h);
    }).modal('show');
    
}



$(document).ready(function () {

    //setup bloodhound engine
    var suggestions = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: './Suggestions.ashx?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    suggestions.initialize();

    $('.txtAutoSearchBox').typeahead(null, {
        name: 'suggestions-ta',
        display: 'value',
        source: suggestions
    });

    $('.txtAutoSearchBox').parent().css('display', '');
});

