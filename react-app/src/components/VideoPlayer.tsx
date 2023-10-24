export interface VideoPlayerProps {
  videoName: string;
  videoUrl: string;
}

export default function VideoPlayer({videoName, videoUrl}: VideoPlayerProps) {
  return (
    <>
      <div className="rounded-top-5 bg-dark pt-3 px-3 text-center">
          <span className="text-white">{videoName}</span>
      </div>
      <div className="rounded-bottom-5 bg-dark pt-3 pb-4 px-3">
        <div className="embed-responsive embed-responsive-16by9">
        <iframe
            className="embed-responsive-item video-player"
            src={videoUrl}
            title="Video"
            allowFullScreen
          ></iframe>
        </div>
      </div>
    </>
  )
}
