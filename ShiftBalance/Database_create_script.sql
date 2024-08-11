-- Database: ShiftBalance

-- DROP DATABASE IF EXISTS "ShiftBalance";

CREATE DATABASE "ShiftBalance"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Italian_Italy.1252'
    LC_CTYPE = 'Italian_Italy.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- TABELLE --
-- Table: public.Employee
DROP TABLE IF EXISTS public."Employee";
CREATE TABLE IF NOT EXISTS public."Employee"
(
    "Id" integer NOT NULL,
    "Name" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "Surname" character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "Profile" smallint NOT NULL,
    "DateOfHiring" date,
    "DateOfBirth" date,
    "ShiftAverage" integer,
    "Openings" integer,
    "Closings" integer,
    "Availability" integer,
    CONSTRAINT "Employee_pkey" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Employee"
    OWNER to postgres;

-- Table: public.EmployeeVacations
DROP TABLE IF EXISTS public."EmployeeVacations";
CREATE TABLE IF NOT EXISTS public."EmployeeVacations"
(
    "ID" integer NOT NULL,
    "IdEmployee" integer NOT NULL,
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    CONSTRAINT "EmployeeVacations_pkey" PRIMARY KEY ("ID"),
    CONSTRAINT "Employee_EmployeeVacations" FOREIGN KEY ("IdEmployee")
        REFERENCES public."Employee" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;