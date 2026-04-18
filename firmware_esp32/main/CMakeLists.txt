idf_component_register(SRCS "main.c" "line_codec.c" "command_processor.c"
                    INCLUDE_DIRS ".")

# Mantiene compatibilidad de rutas históricas (*.c) pero compila como C++ para
# preservar la migración a paradigma OOP/encapsulado del firmware.
set_source_files_properties(
    main.c
    line_codec.c
    command_processor.c
    PROPERTIES LANGUAGE CXX
)
