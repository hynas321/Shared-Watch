import ReactPlayer from "react-player";

export interface VideoPlayerProps {
  videoName: string;
  videoUrl: string;
}

export default function VideoPlayer({videoName, videoUrl}: VideoPlayerProps) {
  return (
    <>
      <div className="rounded-top-5 bg-dark pt-2 text-center">
          <span className="text-white">{videoName}</span>
      </div>
      <div className="d-flex justify-content-center rounded-bottom-5 bg-dark pt-2 pb-5">
        <ReactPlayer
          url={videoUrl}
          playing={false}
          controls={true}
          width={"854px"}
          height={"480px"}
          style={{}}
          onPlay={() => {}}
          onPause={() => {}}
        />
      </div>
    </>
  )
}
