#!/bin/sh
set -e

DOMAIN=${DOMAIN:?DOMAIN is required}
EMAIL=${LETSENCRYPT_EMAIL:?LETSENCRYPT_EMAIL is required}

mkdir -p /var/www/certbot /etc/letsencrypt
certbot certonly --webroot -w /var/www/certbot -d "$DOMAIN" --email "$EMAIL" --agree-tos --no-eff-email
