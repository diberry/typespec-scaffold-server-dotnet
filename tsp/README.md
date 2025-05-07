tsp compile . \
--emit=["@typespec/http-server-csharp","@typespec/openapi3"] \
--output-dir="{project-root}/../my-gen" \
--options=["@typespec/http-server-csharp.use-swaggerui=true","@typespec/http-server-csharp.overwrite=true","@typespec/http-server-csharp.mocks-and-project-files=true"] \
--debug