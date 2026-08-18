-- Criação dos schemas dos microsserviços
CREATE SCHEMA IF NOT EXISTS korp_estoque;
CREATE SCHEMA IF NOT EXISTS korp_faturamento;

-- Sequence para numeração sequencial de Notas Fiscais (thread-safe, sem race condition)
CREATE SEQUENCE IF NOT EXISTS korp_faturamento.nota_fiscal_numero_seq
    START 1
    INCREMENT 1
    NO MAXVALUE
    CACHE 1;
