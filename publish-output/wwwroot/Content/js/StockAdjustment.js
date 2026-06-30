
// modal generation for Purchase Batch Stock
function SBPurModal(result, dataid, type) {
    var ItemQuantity = $('#ItemQuantity').val();
    var BStUnit = "<div class='BStUnit' data-confactor='" + result.ConFactor + "' data-ItemUnitID='" + result.ItemUnitID + "'  data-PriUnit='" + result.PriUnit + "'" +
         " data-SubUnitID='" + result.SubUnitId + "'  data-SubUnit='" + result.SubUnit + "' >" +
         "</div>";
    var Modal = "<div id='batch-" + dataid + "' class='modal fade batch-" + result.ItemID + "' role='dialog' aria-hidden='true'><div class='modal-dialog modal-lg'><div class='modal-content'>" +
        "<div class='modal-header bg-aqua'><button type='button' class='close' data-dismiss='modal' style='font-size:30px;color:red;'>&times;</button><h4>" + result.ItemName + " -<span id='bts_tqty_" + dataid + "'>" + ItemQuantity + "</span> <span id='bts_Unit_" + dataid + "'>" + result.PriUnit + "</span> </h3></div>" +
            "<div class='modal-body'>" +
            "<table class='table table-bordered table-hover batchtbl' id='batchtbl-" + dataid + "'><thead><tr>" +
            "<th class='text-center'>S/N</th><th class='text-center'>Batch No</th><th class='text-center'>MFG</th><th class='text-center'>EXP</th>" +
            "<th class='text-right'>Qty</th><th>Action</th>" +
            "</tr></thead><tbody></tbody><tfoot><tr><th></th><th></th><th></th><th>Total</th><th class='bstotqty text-right'></th><th></th></tr></tfoot></table>" +
            "<div class='form-actions no-color'><input type='button' value='Update' onclick='btsSubmit(" + dataid + ")'  class='btn btn-success col-sm-offset-5'/>" +
            BStUnit + "</div></div></div></div></div>";
    $("#batchStock").append(Modal);
}
// popup batch stock purchase modal
function PopupBatchStock(arg, ItemId) {
    var BID = "#batch-" + arg;
    var bstlength = $(BID).length;
    if (bstlength != 0) {
        if (!$(BID).is(':visible')) {
            $(BID).modal({
                backdrop: 'static',
                keyboard: false
            });
        }
    }
}
// add batch stock row in purchase
var sbcount = 1, btype = '';
sblimits = 500;
function addSBPURRow(arg, ItemId, BtData) {
    if (sbcount == sblimits) alert("You have reached the limit of adding " + sbcount + " inputs In Batch Stock");
    else {
        var slno = $('#batchtbl-' + arg + ' tbody tr').length + 1;
        var BID = "#batch-" + arg;
        var data = "";
        var cfactor = $(BID + " .BStUnit").attr('data-confactor');
        var punit = $(BID + " .BStUnit").attr('data-ItemUnitID');
        var sunit = $(BID + " .BStUnit").attr('data-SubUnitID');
        var punits = $(BID + " .BStUnit").attr('data-PriUnit');
        var sunits = $(BID + " .BStUnit").attr('data-SubUnit');
        var sbunit = parseFloat($('#ItemUnitID').val());
        var gtTot = 0;
        var BStockIn = 0;
        var BStockOut = 0;
        var BEXP = "";
        var BMFG = "";
        var BBatchno = "";
        var BOption = "";
        var SOhide = "";
        var SIhide = "";
        var Type = $('#AdjustmentType').val();
        if (Type == 0) {
            SIhide = " hide";
            SOhide = "";
        } else {
            SOhide = " hide";
            SIhide = "";
        }
        if (BtData) {
            var acSt = parseFloat(BtData.StockIn) / parseFloat(cfactor);
            BStockIn = (sbunit != punit) ? BtData.StockIn : acSt
            var acStO = parseFloat(BtData.StockOut) / parseFloat(cfactor);
            BStockOut = (sbunit != punit) ? BtData.StockOut : acStO;
            var BEXP = BtData.EXPd != null ? convertToDate(BtData.EXPd) : "";
            var BMFG = BtData.MFGd != null ? convertToDate(BtData.MFGd) : "";
            BOption = "<option value='" + BtData.BatchNo + "'>" + BtData.BatchNo + "</option>";
            BBatchno = BtData.BatchNo;
        }
        $("#item_" + arg + " .totrate").each(function () {
            var rate = $(this).val();
            rate = rate || 0;
            gtTot = parseFloat(gtTot) + parseFloat(rate);
        });
        var sbtcost = gtTot;
        var row = "<tr class='Bst_" + slno + "'>";
        data = "<td class='text-center'>" + slno + "</td>" +
           "<td><select data-name='BatchNo' name='bstmodel[" + (slno - 1) + "].BatchNo' data-item='" + ItemId + "' data-count='" + sbcount + "' class='bts_batchno_" + sbcount + " bts_batchno form-control' placeholder='BatchNo' onchange='btsBtch_change(this," + arg + ",\"" + ItemId + "\");' required='required'>" + BOption + "</select></td>" +
           "<td class='date'><input type='text' data-name='MFG' name='bstmodel[" + (slno - 1) + "].MFG' class='bts_mfgdate_" + sbcount + " form-control bts_mfgdate datepicker' value='" + BMFG + "'/></td>" +
           "<td class='date'><input type='text' data-name='EXP' name='bstmodel[" + (slno - 1) + "].EXP' class='bts_expdate_" + sbcount + " form-control bts_expdate datepicker' value='" + BEXP + "'/></td>" +
           "<td><input type='number' data-name='StockIn' name='bstmodel[" + (slno - 1) + "].StockIn' data-count='" + sbcount + "' onchange='btsqty_change(this," + arg + ",\"" + ItemId + "\");' class='bts_qty_" + sbcount + " bts_qnttin form-control text-right" + SIhide + "' placeholder='0' value='" + BStockIn + "' required='required' />" +
           "<input type='number' data-name='StockOut' name='bstmodel[" + (slno - 1) + "].StockOut' data-count='" + sbcount + "' onchange='btsqty_change(this," + arg + ",\"" + ItemId + "\");' class='bts_qty_" + sbcount + " bts_qnttout form-control text-right" + SOhide + "' placeholder='0' value='" + BStockOut + "' required='required' /></td>" +
           "<td class='text-center'><button data-count='" + sbcount + "' class='btn btn-danger' type='button' value='Delete'  onclick='deleteBtsRow(this,\"" + arg + "\")'><i class='fa fa-trash fa-1x'></i></button>" +
           "<input type='hidden' data-name='Item' name='bstmodel[" + (slno - 1) + "].Item' class='bts_item' value='" + ItemId + "'/>" +
           "<input type='hidden' data-name='cfactor' name='bstmodel[" + (slno - 1) + "].cfactor' class='bts_cfactor' value='" + cfactor + "'/>" +
           "<input type='hidden' data-name='Priunit' name='bstmodel[" + (slno - 1) + "].Priunit' class='bts_punit' value='" + punit + "'/>" +
            "<input type='hidden' data-name='Secunit' name='bstmodel[" + (slno - 1) + "].Secunit' class='bts_sunit' value='" + (sunit == null) ? '' : sunit + "'/>" +
           "<input type='hidden' data-name='Cost' name='bstmodel[" + (slno - 1) + "].Cost' class='bts_cost'  value='" + sbtcost + "'/>" +
           "<input type='hidden' data-name='Unit' name='bstmodel[" + (slno - 1) + "].Unit' class='bts_units' id='bts_unit_name_" + sbcount + "' value='" + sbunit + "'>" +
           "<input type='hidden' data-name='Order' name='bstmodel[" + (slno - 1) + "].Order' class='bts_order'  value='" + arg + "'/>" +
           "</td>",
        row += data + "</tr>";
        $('#batchtbl-' + arg + ' tbody').append(row);
        sbcount++;
        totalbtsqty(arg);
        $('.date').datepicker({
            format: 'dd-mm-yyyy',
            autoclose: true,
            allowInputToggle: true,
        });
        BatchNo(ItemId);
    }
}
function BatchNo(item) {
    // Item Category
    $(".bts_batchno").select2({
        placeholder: 'Search Batch No',
        minimumInputLength: 0,
        tags: true,
        ajax: {
            url: "/Item/SearchBatch",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    itemid: $(this).attr("data-item")
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
        templateResult: batchFormatResult,
        templateSelection: batchFormatSelection,
    });
}
function batchFormatResult(repo) {
    var bg = "";
    var markup = '<div class="se-row' + bg + '">' +
             '<h4>' + repo.text + '</h4>';
    markup += '<div class="se-sec">Stock:' + parseFloat(repo.Stock).toFixed(2) + '</div>';
    if (repo.Stock) {

    }


    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function batchFormatSelection(repo) {
    return repo.text;
}

function BStcostup(arg) {
    var gtTot = parseFloat($('#PurchaseRate').val());
    $("#batchtbl-" + arg + " .bts_cost").val(gtTot);
}
function btsBtch_change(t, arg, itemid) {
    var BatchNo = $(t).val();
    $.ajax({
        url: "/Item/GetBatch",
        type: "GET",
        dataType: "JSON",
        data: { BatchNo: BatchNo, itemid: itemid },
        success: function (result) {
            var gp = $(t).parents("tr");
            gp.find('.bts_mfgdate').val(result.MFG);
            gp.find('.bts_expdate').val(result.EXP);
            var dataMax = parseFloat(gp.find('.bts_qntt').attr('data-max')) + parseFloat(result.Stock);
            var max = gp.find('.bts_qntt').attr('max', dataMax);
        }
    });
    btsqty_change(t, arg, itemid);
}
function btsqty_change(t, arg, itemid) {
    var barg = $(t).attr('data-count');
    var flag = "";
    var Type = $('#AdjustmentType').val();

    $("#batchtbl-" + arg + " tbody tr").each(function () {
        var batch = $(this).find(".bts_batchno").val();
        if (Type == 0) {
            var qty = $(this).find('.bts_qnttout').val();
        } else {
            var qty = $(this).find('.bts_qnttin').val();
        }
        if (batch == null || batch == "" || qty <= 0) {
            flag = "nop";
        }
    });
    if (flag != "nop") {
        addSBPURRow(arg, itemid);
    }
    var gp = $(t).parents("tr");
    if (Type == 0) {
        var max = parseFloat(gp.find('.bts_qnttout').attr('max'));
        var min = parseFloat(gp.find('.bts_qnttout').attr('min'));
        var btsQty = parseFloat(gp.find('.bts_qnttout').val());
        if (btsQty > max) {
            gp.find('.bts_qnttout').val(max);
        }
        else if (btsQty < min) {
            gp.find('.bts_qnttout').val(min);
        }
    } else {
        var max = parseFloat(gp.find('.bts_qnttin').attr('max'));
        var min = parseFloat(gp.find('.bts_qnttin').attr('min'));
        var btsQty = parseFloat(gp.find('.bts_qnttin').val());
        if (btsQty > max) {
            gp.find('.bts_qnttin').val(max);
        }
        else if (btsQty < min) {
            gp.find('.bts_qnttin').val(min);
        }
    }
    totalbtsqty(arg);
}
function totalbtsqty(arg) {
    var btsqty = 0;
    var Type = $('#AdjustmentType').val();
    $("#batchtbl-" + arg + " tr").each(function () {
        var bqty = 0;
        if (Type == 0) {
            var bqty = $(this).find('.bts_qnttout').val();
        } else {
            var bqty = $(this).find('.bts_qnttin').val();
        }
        var batch = $(this).find(".bts_batchno").val();
        bqty = bqty || 0;
        btsqty += (batch != "") ? parseFloat(bqty) : 0;
    });
    $("#batchtbl-" + arg + " .bstotqty").text(btsqty.toFixed(2));
}

function btsSubmit(arg) {
    var btsqty = $("#bts_tqty_" + arg).text();
    var itemqty = 0;
    var Type = $('#AdjustmentType').val();
    $("#batchtbl-" + arg + " tr").each(function () {
        var bqty = 0;
        if (Type == 0) {
            var bqty = $(this).find('.bts_qnttout').val();
        } else {
            var bqty = $(this).find('.bts_qnttin').val();
        }
        var batch = $(this).find(".bts_batchno").val();
        bqty = bqty || 0;
        itemqty += (batch != "") ? parseFloat(bqty) : 0;
    });
    if (btsqty != itemqty) {
        alert("Item Quantity and Total batch wise Quantity should be Same ..!! ");
    } else {
        $('#batch-' + arg + '').modal('hide');
    }
}

function deleteBtsRow(t, arg) {
    var barg = $(t).attr('data-count');
    var Type = $('#AdjustmentType').val();
    var qty = 0;
    var batch = $("#batchtbl-" + arg + " .bts_batchno_" + barg).val();
    if (Type == 0) {
        var qty = $("#batchtbl-" + arg + " .bts_qnttout.bts_qty_" + barg).val();
    } else {
        var qty = $("#batchtbl-" + arg + " .bts_qnttin.bts_qty_" + barg).val();
    }
    if (batch != "" && qty > 0) {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
    }
    else {
        alert("Sorry You Can't Delete This Row.");
    }
    totalbtsqty(arg);
}

function stTypeChange() {
    var Type = $('#AdjustmentType').val();
    $("#batchtbl-0 tr").each(function () {
        var bqty = 0;
        if (Type == 0) {
            var bqty = $(this).find('.bts_qnttin').val();
            bqty = bqty || 0;
            $(this).find('.bts_qnttout').val(bqty);
            $(this).find('.bts_qnttin').val(0);
            $(".bts_qnttin").addClass("hide");
            $(".bts_qnttout").removeClass("hide");
        } else {
            var bqty = $(this).find('.bts_qnttout').val();
            bqty = bqty || 0;
            $(this).find('.bts_qnttin').val(bqty);
            $(this).find('.bts_qnttout').val(0);
            $(".bts_qnttout").addClass("hide");
            $(".bts_qnttin").removeClass("hide");
        }
    });
    totalbtsqty(0);
}
function UnitChange() {
    var unitname = $("#ItemUnitID option:selected").text();
    $("#bts_Unit_0").text(unitname);
    $("#batchtbl-0 tr").each(function () {
        var unit = $('#ItemUnitID').val();
        $(this).find('.bts_units').val(unit);
    });
}


function getunit(itemId, unit) {
    //$("[id$=PurchaseRate]").val(1);
    var prate = $("#PurchaseRate").val();
    $.ajax({
        url: '/Item/GetItem',
        type: "GET",
        dataType: "JSON",
        data: { itemID: itemId },
        success: function (data) {
            $("#cofactor").val(data.ConFactor);
            $("#prate").val(data.PurchasePrice);
            var newOption = $('<option></option>');
            if ((data.PriUnit != data.SubUnit) && data.SubUnitId != null) {
                newOption.val(data.ItemUnitID).html(data.PriUnit);
                var newOption1 = $('<option></option>');
                newOption1.val(data.SubUnitId).html(data.SubUnit);
                if (unit) {
                    if (unit == data.ItemUnitID)
                        newOption.attr("selected", "selected");
                    if (unit == data.SubUnitId)
                        newOption1.attr("selected", "selected");
                }
                console.log(newOption);
                $('#ItemUnitID').html('');
                $('#ItemUnitID').append(newOption);
                $('#ItemUnitID').append(newOption1);

            } else {
                $('#ItemUnitID').html('');
                newOption.val(data.ItemUnitID).html(data.PriUnit);
                $('#ItemUnitID').append(newOption);
            }
            if (data.PurchasePrice) {
                    $("#PurchaseRate").val(data.PurchasePrice);
            } else {
                //if (prate == null || prate == "")
                    $("#PurchaseRate").val(1);
            }
            StockValue();
            // batch stock updates
            var BatchEnable = $("#batchcheck").val();
            $("#batch-0").remove();
            if (BatchEnable == "active" && data.KeepStock == true) {
                $("#ItemQuantity").val(0);
                SBPurModal(data, 0);
                addSBPURRow(0, data.ItemID);
            }
        }
    });
}


function StockValue() {   
    var price = parseFloat($("#PurchaseRate").val());
    var quty = parseFloat($("#ItemQuantity").val());
    var stockvalue = price * quty;
    $("#stockvalue").val(stockvalue.toFixed(2));
}

function AssetStockValue() {   
    var price = parseFloat($("#PurchaseRate").val());
    var quty = parseFloat($("#AssetQuantity").val());

    var stockvalue = price * quty;
    $("#stockvalue").val(stockvalue.toFixed(2));
}

//To set The Asset Unit
function GetAssetUnit(AssetId) {
  
    $.ajax({
        url: '/AssetToInventory/GetAssetDetails',
        type: "GET",
        dataType: "JSON",
        data: { AssetId: AssetId },
        success: function (data) {           
            $("#prate").val(data.Price);
            var newOption = $('<option></option>');            
            
                $('#AssetUnitId').html('');
                newOption.val(data.UnitId).html(data.PriUnit);
                $('#AssetUnitId').append(newOption);
           
            if (data.Price) {
                $("#PurchaseRate").val(data.Price.toFixed(2));
            } else {                
                $("#PurchaseRate").val("1.00");
            }
            AssetStockValue();           
        }
    });    
}
