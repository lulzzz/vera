FROM alpine AS build
RUN apk add --no-cache wget
RUN wget -O /tmp/fiskaly-service https://storage.googleapis.com/fiskaly-cdn/clients/com.fiskaly.service-linux-amd64-v1.2.200

FROM alpine
COPY --from=build /tmp/fiskaly-service /fiskaly-service
RUN chmod +x /fiskaly-service
EXPOSE 8080
ENTRYPOINT /fiskaly-service