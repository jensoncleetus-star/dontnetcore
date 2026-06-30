var mobcount = 0;

function AddMobile(Mobile, Name) {
    var previouscount=parseFloat(mobcount)-1;
    var previous = $('#mob_' + previouscount).val();
   
    if (mobcount==0 || previous != "") {
        var mob = $('#mobmodel[' + (mobcount) + '].Num').val();
        Mobile = (Mobile == undefined) ? "" : Mobile;
        Name = (Name == undefined) ? "" : Name;
        if (mobcount == 1 || mob != "") {

            var html = '<div class="input-group mobSet" style="margin-top:10px;">' + '<label class="control-label" style="margin-top:5px;" for="Mobile"> Mobile  </label>'+'&nbsp;' +
                '<input class="form-control text-box single-line mbNum" style="width:45%;" value="' + Mobile + '" id="mob_' + mobcount + '" name="mobmodel[].Num" type="text" placeholder="Enter Mobile">' +'&nbsp;' +
            '<div class="input-group-addon no-padding mobwidth" style="width:50%;"><input class="form-control mbName" value="' + Name + '" id="name_' + mobcount + '" name="mobmodel[' + mobcount + '].Name" type="text" placeholder="Enter Name" /></div>' +
            '<span class="input-group-btn mb-add"  ><button type="button" class="btn " style=" margin-left:4px; background-color:white;" title="Add More"  onclick="AddMobile()"><i class="fa fa-2x fa-plus-circle round-blue-button-mobjs "></i></button></span>' +
                '<span class="input-group-btn mb-dlt hide"><button type="button" class="btn " style=" margin-left:4px;  background-color:white;""   onclick="deleteMobileRow(this,' + mobcount + ')"><i class="fa fa-2x fa-trash trash-red-button" ></i></button></span>' +
            '</div>';
            $(html).appendTo($("#mobiles"));
            mobcount++;
            resetMbbtn();
        }
    }   
}
function resetMbbtn() {
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
function deleteMobileRow(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (mobcount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        mobcount--;
    }
    resetMbbtn();
}