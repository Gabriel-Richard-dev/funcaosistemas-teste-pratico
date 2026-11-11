/*
 * Utilitarios de CPF: mascara (999.999.999-99) e validacao dos digitos verificadores.
 * Obs.: mantido em ES5 por compatibilidade com o minificador do System.Web.Optimization.
 */
var Cpf = (function () {

    function limpar(valor) {
        return (valor || '').replace(/\D/g, '');
    }

    /* Formata progressivamente enquanto o usuario digita */
    function formatar(valor) {
        var d = limpar(valor).substring(0, 11);

        if (d.length > 9)
            return d.replace(/^(\d{3})(\d{3})(\d{3})(\d{1,2})$/, '$1.$2.$3-$4');
        if (d.length > 6)
            return d.replace(/^(\d{3})(\d{3})(\d{1,3})$/, '$1.$2.$3');
        if (d.length > 3)
            return d.replace(/^(\d{3})(\d{1,3})$/, '$1.$2');

        return d;
    }

    function calcularDigito(numeros, quantidade) {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += numeros[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    function validar(valor) {
        var d = limpar(valor);

        if (d.length !== 11 || /^(\d)\1{10}$/.test(d))
            return false;

        var numeros = d.split('').map(Number);

        return numeros[9] === calcularDigito(numeros, 9)
            && numeros[10] === calcularDigito(numeros, 10);
    }

    /* Aplica a mascara ao campo e, opcionalmente, a validacao nativa do formulario (setCustomValidity) */
    function aplicarMascara($campo, validarNoFormulario) {
        $campo.attr('maxlength', 14).attr('inputmode', 'numeric');

        $campo.on('input', function () {
            this.value = formatar(this.value);

            if (validarNoFormulario)
                this.setCustomValidity(this.value === '' || validar(this.value) ? '' : 'Digite um CPF válido');
        });
    }

    return {
        limpar: limpar,
        formatar: formatar,
        validar: validar,
        aplicarMascara: aplicarMascara
    };
})();
