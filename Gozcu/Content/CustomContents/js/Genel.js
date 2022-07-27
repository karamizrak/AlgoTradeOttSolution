PageMethodCagir = function (url, options) {
    var param = options.param == null ? {} : options.param;
    var basariliOlunca = options.basariliOlunca;
    var hataVerince = options.hataVerince == null ? JsonErrorHandler : options.hataVerince;
    var baglantiOncesinde = options.baglantiOncesinde;
    var tamamlaninca = options.tamamlaninca;
    var asyncdurumu = options.asyncdurumu !== false ? true : false;
    try {
        $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(param),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: asyncdurumu,
            success: function (sonuc) { basariliOlunca(sonuc); },
            error: hataVerince,
            beforeSend: baglantiOncesinde,
            complete: tamamlaninca
        });
    } catch (err) {
        JsonErrorHandler(err);
    }
};
function JsonErrorHandler(e) {
    var hata = e.responseJSON;
    MesajGoster("Hata Oluştu.", hata);
}
function MesajGoster(baslik, mesaj) {
    var _modal = $('#modalOrnek');
    var modalBaslik = $('#modalBaslik');
    var modalIcerik = $('#modalIcerik');
    var modalFooter = $('#modalFooter');
    var modalButonKapat = $('#modalButonKapat');
    var modalButonKaydet = $('#modalButonKaydet');
    modalBaslik.html(baslik);
    modalIcerik.html(mesaj);
    modalButonKaydet.hide();
    _modal.modal();
    _modal.modal('show');
}
function UyariGetir(baslik, mesaj, footerVarmi, kapatButonVarmi, kapatButonYazi, footerEkstra) {
    var _modal = $('#modalOrnek');
    var modalBaslik = $('#modalBaslik');
    var modalIcerik = $('#modalIcerik');
    var modalFooter = $('#modalFooter');
    var modalButonKapat = $('#modalButonKapat');
    var modalButonKaydet = $('#modalButonKaydet');
    _modal.modal();
    modalBaslik.html(baslik);
    modalIcerik.html(mesaj);
    if (footerVarmi === false) {
        modalFooter.hide();
    }
    var htm = '';
    if (kapatButonVarmi === true) {
        var _kapatYazi = "Kapat";
        if (kapatButonYazi !== "") {
            _kapatYazi = kapatButonYazi;
        }
        htm += '<button type="button" data-dismiss="modal" class="btn btn-secondary" id="modalButonKapat">' + _kapatYazi + '</button>';
    }
    if (footerEkstra !== "") {
        htm += footerEkstra;
    }
    modalFooter.html(htm);
    modalButonKaydet.hide();
    _modal.modal('show');
}

