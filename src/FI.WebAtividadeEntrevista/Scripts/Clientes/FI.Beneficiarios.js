/*
 * Manutencao dos beneficiarios do cliente (pop-up "Beneficiarios").
 * Os beneficiarios sao mantidos em memoria e enviados ao servidor
 * junto com o cliente quando o botao "Salvar" da tela e acionado.
 * Obs.: mantido em ES5 por compatibilidade com o minificador do System.Web.Optimization.
 */
var Beneficiarios = (function () {

    var lista = [];          // { Id, CPF, Nome }
    var indiceEdicao = -1;   // indice do beneficiario em alteracao (-1 = inclusao)

    var $modal, $cpf, $nome, $botao, $alerta, $corpoGrid;

    function init() {
        $modal = $('#modalBeneficiarios');
        $cpf = $('#CPFBeneficiario');
        $nome = $('#NomeBeneficiario');
        $botao = $('#btnIncluirBeneficiario');
        $alerta = $('#alertaBeneficiario');
        $corpoGrid = $('#gridBeneficiarios tbody');

        Cpf.aplicarMascara($cpf, false);

        $botao.on('click', salvarItem);

        $cpf.add($nome).on('keydown', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                e.preventDefault();
                salvarItem();
            }
        });

        $corpoGrid.on('click', '.btn-alterar-beneficiario', function () {
            editarItem(Number($(this).closest('tr').data('indice')));
        });

        $corpoGrid.on('click', '.btn-excluir-beneficiario', function () {
            excluirItem(Number($(this).closest('tr').data('indice')));
        });

        $modal.on('shown.bs.modal', function () { $cpf.trigger('focus'); });
        $modal.on('hidden.bs.modal', limparCampos);

        renderizar();
    }

    function salvarItem() {
        var cpf = $.trim($cpf.val());
        var nome = $.trim($nome.val());

        if (!cpf || !nome)
            return mostrarErro('Informe o CPF e o nome do beneficiário.');

        if (!Cpf.validar(cpf))
            return mostrarErro('Digite um CPF válido.');

        if (nome.length > 50)
            return mostrarErro('O nome do beneficiário deve possuir no máximo 50 caracteres.');

        var cpfLimpo = Cpf.limpar(cpf);
        for (var i = 0; i < lista.length; i++) {
            if (i !== indiceEdicao && Cpf.limpar(lista[i].CPF) === cpfLimpo)
                return mostrarErro('Já existe um beneficiário com este CPF para este cliente.');
        }

        if (indiceEdicao >= 0) {
            lista[indiceEdicao].CPF = Cpf.formatar(cpf);
            lista[indiceEdicao].Nome = nome;
        } else {
            lista.push({ Id: 0, CPF: Cpf.formatar(cpf), Nome: nome });
        }

        limparCampos();
        renderizar();
        $cpf.trigger('focus');
    }

    function editarItem(indice) {
        var item = lista[indice];
        if (!item)
            return;

        indiceEdicao = indice;
        $cpf.val(item.CPF);
        $nome.val(item.Nome);
        $botao.text('Alterar');
        esconderErro();
        renderizar();
        $cpf.trigger('focus');
    }

    function excluirItem(indice) {
        if (indice === indiceEdicao)
            limparCampos();
        else if (indice < indiceEdicao)
            indiceEdicao--;

        lista.splice(indice, 1);
        renderizar();
    }

    function limparCampos() {
        indiceEdicao = -1;
        $cpf.val('');
        $nome.val('');
        $botao.text('Incluir');
        esconderErro();
        renderizar();
    }

    function renderizar() {
        $corpoGrid.empty();

        if (lista.length === 0) {
            $corpoGrid.append(
                $('<tr>').append($('<td colspan="3" class="text-center text-muted">').text('Nenhum beneficiário incluído.')));
            return;
        }

        $.each(lista, function (indice, item) {
            var $linha = $('<tr>').attr('data-indice', indice).toggleClass('info', indice === indiceEdicao);

            $linha.append($('<td>').text(item.CPF));
            $linha.append($('<td>').text(item.Nome));
            $linha.append($('<td class="text-nowrap">').append(
                $('<button type="button" class="btn btn-sm btn-primary btn-alterar-beneficiario">').text('Alterar'),
                ' ',
                $('<button type="button" class="btn btn-sm btn-primary btn-excluir-beneficiario">').text('Excluir')));

            $corpoGrid.append($linha);
        });
    }

    function mostrarErro(mensagem) {
        $alerta.text(mensagem).removeClass('hidden');
        return false;
    }

    function esconderErro() {
        $alerta.text('').addClass('hidden');
    }

    /* Carrega os beneficiarios ja cadastrados (tela de alteracao) */
    function carregar(beneficiarios) {
        lista = $.map(beneficiarios || [], function (b) {
            return { Id: b.Id, CPF: Cpf.formatar(b.CPF), Nome: b.Nome };
        });

        if ($corpoGrid) // se ainda nao inicializado, o init() renderiza a lista
            limparCampos();
    }

    /* Retorna a lista para envio ao servidor */
    function obter() {
        return $.map(lista, function (b) {
            return { Id: b.Id, CPF: b.CPF, Nome: b.Nome };
        });
    }

    function limpar() {
        carregar([]);
    }

    $(document).ready(init);

    return {
        carregar: carregar,
        obter: obter,
        limpar: limpar
    };
})();
