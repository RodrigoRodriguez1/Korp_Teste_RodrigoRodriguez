-- Criação dos schemas dos microsserviços
CREATE SCHEMA IF NOT EXISTS estoque;
CREATE SCHEMA IF NOT EXISTS faturamento;

-- Sequence para numeração sequencial de Notas Fiscais (thread-safe, sem race condition)
CREATE SEQUENCE IF NOT EXISTS faturamento.nota_fiscal_numero_seq
    START 1
    INCREMENT 1
    NO MAXVALUE
    CACHE 1;
