# Finamon Backend

## Environment Setup

1. Create a `.env` file in the root directory with the following variables:

```env
FIREBASE_CONFIG={"type":"service_account",...} # Your Firebase service account credentials
FIREBASE_STORAGE_BUCKET=your-bucket-name.appspot.com
```

2. Never commit the `.env` file or `firebase-adminsdk.json` to git

## Development

1. Install dependencies
2. Set up environment variables
3. Run the application

## Firebase Setup

1. Go to Firebase Console
2. Generate a new Service Account key
3. Copy the JSON content to FIREBASE_CONFIG in .env
4. Set FIREBASE_STORAGE_BUCKET to your bucket name
