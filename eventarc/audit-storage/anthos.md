# Events for Cloud Run on Anthos – GCS tutorial

This sample shows how to create a service that processes GCS events on Anthos.
It is assumed that you already created a GKE cluster with Events for Cloud Run
already installed.

## Setup

Configure environment variables:

```sh
MY_RUN_SERVICE=gcs-service
MY_RUN_CONTAINER=gcs-container
MY_GCS_BUCKET=gcs-bucket-$(gcloud config get-value project)
```

## Quickstart

Set cluster name, location and platform:

```sh
gcloud config set run/cluster events-cluster
gcloud config set run/cluster_location us-central1-b
gcloud config set run/platform gke
```

Deploy your Cloud Run service:

```sh
gcloud builds submit \
 --tag gcr.io/$(gcloud config get-value project)/${MY_RUN_CONTAINER}
gcloud run deploy ${MY_RUN_SERVICE} \
 --image gcr.io/$(gcloud config get-value project)/${MY_RUN_CONTAINER}
```

Create a bucket:

```sh
gsutil mb -p $(gcloud config get-value project) \
    -l us-central1 \
    gs://${MY_GCS_BUCKET}
```

Before creating a trigger, you need to give the default service account for
Cloud Storage permission to publish to Pub/Sub.

Find the Service Account that Cloud Storage uses to publish
to Pub/Sub. You can use the steps outlined in [Cloud Console or the JSON
API](https://cloud.google.com/storage/docs/getting-service-account). Assume the
service account you found from above was
`service-XYZ@gs-project-accounts.iam.gserviceaccount.com`, set this to an
environment variable:

```sh
export GCS_SERVICE_ACCOUNT=service-XYZ@gs-project-accounts.iam.gserviceaccount.com
gcloud projects add-iam-policy-binding $(gcloud config get-value project) \
--member=serviceAccount:${GCS_SERVICE_ACCOUNT} \
--role roles/pubsub.publisher
```

Create Cloud Storage trigger:

```sh
gcloud beta events triggers create my-gcs-trigger \
  --target-service ${MY_RUN_SERVICE} \
  --type=google.cloud.storage.object.v1.finalized \
  --parameters bucket=${MY_GCS_BUCKET}
```

## Test

Test your Cloud Run service by uploading a file to the bucket:

```sh
echo "Hello World" > random.txt
gsutil cp random.txt gs://${MY_GCS_BUCKET}/random.txt
```

Observe the Cloud Run service printing upon receiving an event in
Cloud Logging:

```sh
gcloud logging read "resource.type=cloud_run_revision AND \
resource.labels.service_name=${MY_RUN_SERVICE}" --project \
$(gcloud config get-value project) --limit 30 --format 'value(textPayload)'
```
