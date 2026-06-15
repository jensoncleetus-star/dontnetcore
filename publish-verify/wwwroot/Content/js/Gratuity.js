var newcount = 1, type = '';
limits = 500;
function AddGratuity(t, action, from, to, days) {

    if (newcount == limits) alert("You have reached the limit of adding " + newcount + " inputs");
    else {
        var Option = "";
        var required = "";
        var divid = "gratuity_" + from;
        var data = "";
        var a = "item_name" + newcount,
        tabindex = newcount * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        var slno = $('#gratuitybody tr').length + 1;
        var row = "<tr class='gratuity_" + newcount + "' id='gratuity_" + newcount + "'>";
        data =
                "<td style='width:25px;'><input  name='gratmodel[" + (newcount - 1) + "].datefrom' tabindex='" + tab1 + "' data-msg-required='The from field is required' value='" + from + "' id='from_" + newcount + "' value='' class='from_" + newcount + " form-control text-left from' placeholder='Enter from' min='0''/></td> " +
                "<td style='width:25px;'><input  name='gratmodel[" + (newcount - 1) + "].dateto' tabindex='" + tab2 + "' data-msg-required='The to field is required' value='" + to + "' id='to_" + newcount + "' onchange='addnxtdata(this," + newcount + ")' class='to_" + newcount + " form-control text-left to' placeholder='Enter to' min='0''/></td> " +
                "<td style='width:50px;'><input  name='gratmodel[" + (newcount - 1) + "].days' tabindex='" + tab3 + "' data-msg-required='The days field is required' value='" + days + "' id='days_" + newcount + "' value='' class='days_" + newcount + " form-control text-left days' placeholder='Enter days' min='0''/></td> ";

               // "<td style='width:50px;' class='text-center'><span class='input-group-btn mb-add'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-default btn-success' type='button' value='Add' onclick='addRow(this," + newcount + ")'><i class='fa fa-1x fa-plus-circle'></i></span>"
               // "<span class='input-group-btn mb-dlt hide'><button tabindex='" + tab4 + "' style='text-align: right;' class='btn btn-default btn-danger' type='button' value='Add' onclick='addRow(this," + newcount + ")'><i class='fa fa-1x fa-plus-circle'></i></span></td>";
        "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        
    }
}
function addRow() {
    var i = 0;
    var mbLen = $(".mobSet").length;
    $('.mobSet').each(function (index, element) {
        var inputMb = $(this).find('.mbNum');
        inputMb.attr('name', 'mobmodel[' + i + '].Num');
        var dltbtn = $(this).find('.mb-dlt');
        var addbtn = $(this).find('.mb-add');
        if (index === (mbLen - 1)) {
            if (addbtn.hasClass('hide')) {
                addbtn.removeClass('hide');
            }
            if (!dltbtn.hasClass('hide')) {
                dltbtn.addClass('hide');
            }
        }
        else {
            if (!addbtn.hasClass('hide')) {
                addbtn.addClass('hide');
            }
            if (dltbtn.hasClass('hide')) {
                dltbtn.removeClass('hide');
            }
        }
        i++;
    });

}

function deletegratuityRow(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (gracount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        gracount--;
    }
    resetMbbtn();
}
function addRow(object, arg) {
    var data = $('#from_' + arg).val();
    if (($('#gratuitybody tr').length == arg) && data != "") {
        AddGratuity('gratuitybody', 'contact', "", "", "");
    }
}


function addnxtdata(e,arg) {  //  alert($(e).closest('tr').attr('class'))
    var newrow = parseInt(arg);
    var datato = $('#to_' + arg).val(); 
    if (($('#gratuitybody tr').length == arg) && datato != "") {
        newcount++;
        AddGratuity('gratuitybody', 'contact', datato, "", "");
    }
}
